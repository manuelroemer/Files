namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.WindowsStorage.Utilities;
    using Files.Shared.PhysicalStoragePath;
    using StorageFile = StorageFile;
    using StorageFolder = StorageFolder;
    using IOFileAttributes = System.IO.FileAttributes;
    using WinStorageFile = Windows.Storage.StorageFile;
    using WinStorageFolder = Windows.Storage.StorageFolder;
    using WinCreationCollisionOption = Windows.Storage.CreationCollisionOption;
    using Windows.Storage;
    using CreationCollisionOption = CreationCollisionOption;
    using NameCollisionOption = NameCollisionOption;
    using IOPath = System.IO.Path;
    using Files.FileSystems.WindowsStorage.Resources;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices.WindowsRuntime;
    using WinUnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
    using Windows.Media.Protection.PlayReady;

    internal sealed class WindowsStorageStorageFile : StorageFile
    {
        private readonly StoragePath _path;
        private readonly StoragePath _fullPath;
        private readonly StoragePath _fullParentPath;

        public override FileSystem FileSystem { get; }

        public override StoragePath Path => _path;

        public WindowsStorageStorageFile(FileSystem fileSystem, PhysicalStoragePath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            if (path.FullPath.Parent is null)
            {
                throw new ArgumentException(
                    ExceptionStrings.File.CannotInitializeWithRootFolderPath(),
                    nameof(path)
                );
            }

            FileSystem = fileSystem;
            _path = path;
            _fullPath = Path.FullPath;
            _fullParentPath = path.FullPath.Parent;
        }

        public override async Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var props = await file.GetBasicPropertiesAsync().Cancel(cancellationToken);
            var lastModified = props.DateModified == default ? (DateTimeOffset?)null : props.DateModified;

            return new StorageFileProperties(
                file.Name,
                IOPath.GetFileNameWithoutExtension(file.Name),
                PhysicalPathHelper.GetExtensionWithoutTrailingExtensionSeparator(file.Name)?.ToNullIfEmpty(),
                file.DateCreated,
                lastModified,
                props.Size
            );
        }

        public override async Task<IOFileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            return file.Attributes.ToIOFileAttributes();
        }

        public override Task SetAttributesAsync(IOFileAttributes attributes, CancellationToken cancellationToken = default)
        {
            // There's no "native" API for setting file/folder attributes.
            // We can at least try to use System.IO's API - it should at least work in certain locations
            // like the application data.

            return Task.Run(async () =>
            {
                // Get the file to ensure that it exists and to throw the appropriate exception.
                await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
                File.SetAttributes(_fullPath.ToString(), attributes);
            });
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                // IOException might be thrown if a conflicting folder exists.
                // In such cases the specification requires us to return false.
                try
                {
                    await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                    return false;
                }
                catch
                {
                    // No conflicting folder exists. Rethrow the original IOException.
                }
                throw;
            }
        }

        public override async Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            WinStorageFolder parentFolder;
            if (recursive)
            {
                parentFolder = await FsHelper
                    .GetOrCreateFolderAsync(_fullParentPath, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                parentFolder = await FsHelper
                    .GetFolderAsync(_fullParentPath, cancellationToken)
                    .ConfigureAwait(false);
            }

            await parentFolder.CreateFileAsync(_fullPath.Name).Cancel(cancellationToken);
        }

        public override async Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (destinationPath.FullPath.Parent is null)
            {
                throw new IOException(ExceptionStrings.File.CannotMoveOrCopyIntoRootLocation());
            }

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var destFolder = await FsHelper
                .GetFolderAsync(destinationPath.FullPath.Parent, cancellationToken)
                .ConfigureAwait(false);
            await file
                .CopyAsync(destFolder, destinationPath.FullPath.Parent, options.ToWinOptions())
                .Cancel(cancellationToken);
            return FileSystem.GetFile(destinationPath);
        }

        public override async Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (destinationPath.FullPath.Parent is null)
            {
                throw new IOException(ExceptionStrings.File.CannotMoveOrCopyIntoRootLocation());
            }

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var destFolder = await FsHelper
                .GetFolderAsync(destinationPath.FullPath.Parent, cancellationToken)
                .ConfigureAwait(false);
            await file
                .MoveAsync(destFolder, destinationPath.FullPath.Parent, options.ToWinOptions())
                .Cancel(cancellationToken);
            return FileSystem.GetFile(destinationPath);
        }

        public override async Task<StorageFile> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            if (newName.Length == 0)
            {
                throw new ArgumentException(ExceptionStrings.String.CannotBeEmpty(), nameof(newName));
            }

            if (newName.Contains(PhysicalPathHelper.InvalidNewNameCharacters))
            {
                throw new ArgumentException(ExceptionStrings.File.NewNameContainsInvalidChar(), nameof(newName));
            }

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            await file.RenameAsync(newName, options.ToWinOptions()).Cancel(cancellationToken);
            return FileSystem.GetFile(_fullParentPath.Join(newName));
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            return options switch
            {
                DeletionOption.Fail => FailImpl(),
                DeletionOption.IgnoreMissing => IgnoreMissingImpl(),
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
            };

            async Task FailImpl()
            {
                var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete).Cancel(cancellationToken);
            }

            async Task IgnoreMissingImpl()
            {
                try
                {
                    var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                    if (file is object)
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete).Cancel(cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
                {
                    // Nothing to do, since the options allow this case.
                }
            }
        }

        public override async Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var fileAccessMode = fileAccess switch
            {
                FileAccess.Read => FileAccessMode.Read,
                FileAccess.Write => FileAccessMode.ReadWrite,
                FileAccess.ReadWrite => FileAccessMode.ReadWrite,
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(fileAccess)),
            };

            var randomAccessStream = await file.OpenAsync(fileAccessMode).Cancel(cancellationToken);
            return randomAccessStream.AsStream();
        }

        public override async Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            using var stream = await OpenAsync(FileAccess.Read).ConfigureAwait(false);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }

        public override async Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            await FileIO.WriteBytesAsync(file, bytes).Cancel(cancellationToken);
        }

        public override async Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);

            // FileIO doesn't natively provide a lot of encodings.
            // We use the native API when possible, but might fallback to custom conversions if required.
            if (encoding is null)
            {
                return await FileIO.ReadTextAsync(file).Cancel(cancellationToken);
            }
            else if (encoding == Encoding.UTF8)
            {
                return await FileIO.ReadTextAsync(file, WinUnicodeEncoding.Utf8).Cancel(cancellationToken);
            }
            else if (encoding == Encoding.Unicode)
            {
                return await FileIO.ReadTextAsync(file, WinUnicodeEncoding.Utf16LE).Cancel(cancellationToken);
            }
            else if (encoding == Encoding.BigEndianUnicode)
            {
                return await FileIO.ReadTextAsync(file, WinUnicodeEncoding.Utf16BE).Cancel(cancellationToken);
            }
            else
            {
                var buffer = await FileIO.ReadBufferAsync(file).Cancel(cancellationToken);
                var bytes = buffer.ToArray();
                return encoding.GetString(bytes);
            }
        }

        public override async Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        )
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);

            // FileIO doesn't natively provide a lot of encodings.
            // We use the native API when possible, but might fallback to custom conversions if required.
            if (encoding is null)
            {
                await FileIO.WriteTextAsync(file, text).Cancel(cancellationToken);
            }
            else if (encoding == Encoding.UTF8)
            {
                await FileIO.WriteTextAsync(file, text, WinUnicodeEncoding.Utf8).Cancel(cancellationToken);
            }
            else if (encoding == Encoding.Unicode)
            {
                await FileIO.WriteTextAsync(file, text, WinUnicodeEncoding.Utf16LE).Cancel(cancellationToken);
            }
            else if (encoding == Encoding.BigEndianUnicode)
            {
                await FileIO.WriteTextAsync(file, text, WinUnicodeEncoding.Utf16BE).Cancel(cancellationToken);
            }
            else
            {
                await FileIO.WriteBytesAsync(file, encoding.GetBytes(text)).Cancel(cancellationToken);
            }
        }
    }
}
