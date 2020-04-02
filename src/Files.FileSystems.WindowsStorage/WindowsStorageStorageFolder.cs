namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.WindowsStorage.Resources;
    using Files.FileSystems.WindowsStorage.Utilities;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using Windows.Storage;
    using CreationCollisionOption = CreationCollisionOption;
    using IOFileAttributes = System.IO.FileAttributes;
    using IOPath = System.IO.Path;
    using NameCollisionOption = NameCollisionOption;
    using StorageFile = StorageFile;
    using StorageFolder = StorageFolder;
    using WinStorageFolder = Windows.Storage.StorageFolder;

    internal sealed class WindowsStorageStorageFolder : StorageFolder
    {
        private readonly StoragePath _path;
        private readonly StoragePath _fullPath;
        private readonly StoragePath? _fullParentPath;

        public override FileSystem FileSystem { get; }

        public override StoragePath Path => _path;

        public WindowsStorageStorageFolder(FileSystem fileSystem, PhysicalStoragePath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            FileSystem = fileSystem;
            _path = path;
            _fullPath = Path.FullPath;
            _fullParentPath = _fullPath.Parent;
        }

        public override async Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var props = await folder.GetBasicPropertiesAsync().Cancel(cancellationToken);
            var lastModified = props.DateModified == default ? (DateTimeOffset?)null : props.DateModified;

            return new StorageFolderProperties(
                folder.Name,
                IOPath.GetFileNameWithoutExtension(folder.Name),
                PhysicalPathHelper.GetExtensionWithoutTrailingExtensionSeparator(folder.Name)?.ToNullIfEmpty(),
                folder.DateCreated,
                lastModified
            );
        }

        public override async Task<IOFileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            return folder.Attributes.ToIOFileAttributes();
        }

        public override Task SetAttributesAsync(IOFileAttributes attributes, CancellationToken cancellationToken = default)
        {
            // There's no "native" API for setting file/folder attributes.
            // We can at least try to use System.IO's API - it should at least work in certain locations
            // like the application data.

            return Task.Run(async () =>
            {
                try
                {
                    // Get the folder to ensure that it exists and to throw the appropriate exception.
                    await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();
                    File.SetAttributes(_fullPath.ToString(), attributes);
                }
                catch (FileNotFoundException ex)
                {
                    // Since we're using a File API, we must manually convert the FileNotFoundException.
                    throw new DirectoryNotFoundException(message: null, ex);
                }
            });
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                // IOException might be thrown if a conflicting file exists.
                // In such cases the specification requires us to return false.
                try
                {
                    await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                    return false;
                }
                catch
                {
                    // No conflicting file exists. Rethrow the original IOException.
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
            // We cannot reasonably create a root directory with the API.
            // If someone tries to do so, we'll simply deny the call. In most cases, the root
            // folder will exist anyway.
            if (_fullParentPath is null)
            {
                throw new UnauthorizedAccessException();
            }

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
                .CreateFolderAsync(_fullPath.Name, options.ToWinOptions())
                .Cancel(cancellationToken);
        }

        public override async Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (destinationPath.FullPath.Parent is null)
            {
                throw new IOException(ExceptionStrings.Folder.CannotMoveIntoRootLocation());
            }

            var destinationParentFolder = await FsHelper
                .GetFolderAsync(destinationPath.FullPath.Parent, cancellationToken)
                .ConfigureAwait(false);
            var sourceFolder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            await Impl(sourceFolder, destinationParentFolder, destinationPath.FullPath.Name).ConfigureAwait(false);
            return FileSystem.GetFolder(destinationPath.FullPath);

            async Task Impl(WinStorageFolder src, WinStorageFolder dstFolderParent, string dstFolderName)
            {
                var dstFolder = await dstFolderParent
                    .CreateFolderAsync(dstFolderName, ((CreationCollisionOption)options).ToWinOptions())
                    .Cancel(cancellationToken);

                foreach (var file in await src.GetFilesAsync().Cancel(cancellationToken))
                {
                    await file.CopyAsync(dstFolderParent, file.Name).Cancel(cancellationToken);
                }

                foreach (var folder in await src.GetFoldersAsync().Cancel(cancellationToken))
                {
                    await Impl(folder, dstFolder, folder.Name).ConfigureAwait(false);
                }
            }
        }

        public override async Task<StorageFolder> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));

            // There is no native Move API. The current "best practice" (haha) is to simply copy
            // a folder instead of moving.
            // We might be able to improve performance if we're moving into the same directory, i.e.
            // if we're effectively doing a rename.
            // Since this is path based, we have to be careful, of course.
            if (destinationPath.FullPath.Parent == _fullParentPath)
            {
                await RenameAsync(destinationPath.FullPath.Name, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await CopyAsync(destinationPath, cancellationToken).ConfigureAwait(false);
            }

            await DeleteAsync(DeletionOption.IgnoreMissing).ConfigureAwait(false);
            return FileSystem.GetFolder(destinationPath.FullPath);
        }

        public override async Task<StorageFolder> RenameAsync(
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
                throw new ArgumentException(ExceptionStrings.Folder.NewNameContainsInvalidChar(), nameof(newName));
            }

            var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            await folder.RenameAsync(newName, options.ToWinOptions()).Cancel(cancellationToken);
            return FileSystem.GetFolder(_fullParentPath?.Join(newName) ?? newName);
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
                var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete).Cancel(cancellationToken);
            }

            async Task IgnoreMissingImpl()
            {
                try
                {
                    var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                    if (folder is object)
                    {
                        await folder.DeleteAsync(StorageDeleteOption.PermanentDelete).Cancel(cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
                {
                    // Nothing to do, since the options allow this case.
                }
            }
        }

        public override async Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var allFiles = await folder.GetFilesAsync().Cancel(cancellationToken);
            return allFiles.Select(winStorageFolder => FileSystem.GetFile(winStorageFolder.Path));
        }

        public override async Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var allFolders = await folder.GetFoldersAsync().Cancel(cancellationToken);
            return allFolders.Select(winStorageFolder => FileSystem.GetFolder(winStorageFolder.Path));
        }
    }
}
