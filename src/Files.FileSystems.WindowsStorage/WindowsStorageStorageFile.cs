namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.WindowsStorage.Utilities;
    using Files.Shared;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using Windows.Storage;
    using CreationCollisionOption = CreationCollisionOption;
    using IOFileAttributes = System.IO.FileAttributes;
    using IOPath = System.IO.Path;
    using NameCollisionOption = NameCollisionOption;
    using StorageFile = StorageFile;
    using WinStorageFolder = Windows.Storage.StorageFolder;
    using WinUnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

    internal sealed class WindowsStorageStorageFile : StorageFile
    {
        private readonly StoragePath _fullPath;
        private readonly StoragePath _fullParentPath;

        public WindowsStorageStorageFile(WindowsStorageFileSystem fileSystem, PhysicalStoragePath path)
            : base(fileSystem, path)
        {
            _fullPath = Path.FullPath;
            _fullParentPath = path.FullPath.Parent!;
        }

        public override async Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var props = await file.GetBasicPropertiesAsync().AsAwaitable(cancellationToken);
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
            // We can try to use System.IO's API - it should at least work in certain locations
            // like the application data.
            if (!EnumInfo.IsDefined(attributes))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(attributes), nameof(attributes));
            }

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
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            await EnsureNoConflictingFolderExistsAsync(cancellationToken).ConfigureAwait(false);

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

            await parentFolder
                .CreateFileAsync(_fullPath.Name, options.ToWinOptions())
                .AsTask(cancellationToken)
                .WithConvertedException()
                .ConfigureAwait(false);
        }

        public override async Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            if (!(destinationPath is PhysicalStoragePath))
            {
                throw new ArgumentException(
                    ExceptionStrings.FsCompatibility.StoragePathTypeNotSupported(),
                    nameof(destinationPath)
                );
            }

            if (destinationPath.FullPath.Parent is null)
            {
                throw new IOException(ExceptionStrings.StorageFile.CannotMoveToRootLocation());
            }

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var destFolder = await FsHelper
                .GetFolderAsync(destinationPath.FullPath.Parent, cancellationToken)
                .ConfigureAwait(false);
            await file
                .CopyAsync(destFolder, destinationPath.FullPath.Name, options.ToWinOptions())
                .AsTask(cancellationToken)
                .WithConvertedException()
                .ConfigureAwait(false);
            return FileSystem.GetFile(destinationPath);
        }

        public override async Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            if (!(destinationPath is PhysicalStoragePath))
            {
                throw new ArgumentException(
                    ExceptionStrings.FsCompatibility.StoragePathTypeNotSupported(),
                    nameof(destinationPath)
                );
            }

            if (destinationPath.FullPath.Parent is null)
            {
                throw new IOException(ExceptionStrings.StorageFile.CannotMoveToRootLocation());
            }

            var fullDestinationPath = destinationPath.FullPath;
            var destinationFile = FileSystem.GetFile(fullDestinationPath);

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var destFolder = await FsHelper
                .GetFolderAsync(fullDestinationPath.Parent, cancellationToken)
                .ConfigureAwait(false);
            await file
                .MoveAsync(destFolder, fullDestinationPath.Name, options.ToWinOptions())
                .AsTask(cancellationToken)
                .WithConvertedException()
                .ConfigureAwait(false);
            return destinationFile;
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
                throw new ArgumentException(
                    ExceptionStrings.StorageFile.NewNameContainsInvalidChar(FileSystem.PathInformation),
                    nameof(newName)
                );
            }

            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            var destinationPath = _fullParentPath.Join(newName).FullPath;
            var destinationFile = FileSystem.GetFile(destinationPath);

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            await file
                .RenameAsync(newName, options.ToWinOptions())
                .AsTask(cancellationToken)
                .WithConvertedException()
                .ConfigureAwait(false);
            return destinationFile;
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            return options switch
            {
                DeletionOption.Fail => FailImpl(),
                DeletionOption.IgnoreMissing => IgnoreMissingImpl(),
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
            };

            async Task FailImpl()
            {
                var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete).AsAwaitable(cancellationToken);
            }

            async Task IgnoreMissingImpl()
            {
                try
                {
                    var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                    if (file is object)
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete).AsAwaitable(cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                {
                    // Nothing to do, since the options allow this case.
                }
            }
        }

        public override async Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(fileAccess))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(fileAccess), nameof(fileAccess));
            }

            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var fileAccessMode = fileAccess switch
            {
                FileAccess.Read => FileAccessMode.Read,
                FileAccess.Write => FileAccessMode.ReadWrite,
                FileAccess.ReadWrite => FileAccessMode.ReadWrite,
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(fileAccess)),
            };

            var randomAccessStream = await file.OpenAsync(fileAccessMode).AsAwaitable(cancellationToken);
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
            await FileIO.WriteBytesAsync(file, bytes).AsAwaitable(cancellationToken);
        }

        public override async Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            var file = await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);

            // FileIO doesn't natively provide a lot of encodings.
            // We use the native API when possible, but might fallback to custom conversions if required.
            if (encoding is null)
            {
                return await FileIO.ReadTextAsync(file).AsAwaitable(cancellationToken);
            }
            else if (encoding == Encoding.UTF8)
            {
                return await FileIO.ReadTextAsync(file, WinUnicodeEncoding.Utf8).AsAwaitable(cancellationToken);
            }
            else if (encoding == Encoding.Unicode)
            {
                return await FileIO.ReadTextAsync(file, WinUnicodeEncoding.Utf16LE).AsAwaitable(cancellationToken);
            }
            else if (encoding == Encoding.BigEndianUnicode)
            {
                return await FileIO.ReadTextAsync(file, WinUnicodeEncoding.Utf16BE).AsAwaitable(cancellationToken);
            }
            else
            {
                var buffer = await FileIO.ReadBufferAsync(file).AsAwaitable(cancellationToken);
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
                await FileIO.WriteTextAsync(file, text).AsAwaitable(cancellationToken);
            }
            else if (encoding == Encoding.UTF8)
            {
                await FileIO.WriteTextAsync(file, text, WinUnicodeEncoding.Utf8).AsAwaitable(cancellationToken);
            }
            else if (encoding == Encoding.Unicode)
            {
                await FileIO.WriteTextAsync(file, text, WinUnicodeEncoding.Utf16LE).AsAwaitable(cancellationToken);
            }
            else if (encoding == Encoding.BigEndianUnicode)
            {
                await FileIO.WriteTextAsync(file, text, WinUnicodeEncoding.Utf16BE).AsAwaitable(cancellationToken);
            }
            else
            {
                await FileIO.WriteBytesAsync(file, encoding.GetBytes(text)).AsAwaitable(cancellationToken);
            }
        }

        private async Task EnsureNoConflictingFolderExistsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Ideally we'd catch more specific exceptions here.
                // Since this method is only called for throwing the *correct* exception type though,
                // we can be less strict about it.
                // At the end of the day, if a conflicting folder does exist, the I/O APIs *will*
                // throw, just not a guaranteed IOException. No need to make our life harder with
                // extensive exception checking.
                return;
            }

            throw new IOException(ExceptionStrings.StorageFile.ConflictingFolderExistsAtFileLocation());
        }
    }
}
