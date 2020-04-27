namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.Shared;
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

        public WindowsStorageStorageFolder(WindowsStorageFileSystem fileSystem, PhysicalStoragePath path)
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
            var props = await folder.GetBasicPropertiesAsync().AsAwaitable(cancellationToken);
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
            if (!EnumInfo.IsDefined(attributes))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(attributes), nameof(attributes));
            }

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
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            // We cannot reasonably create a root directory with the Windows Storage API.
            // If someone tries to do so, we'll simply deny the call. In most cases, the root
            // folder will exist anyway.
            if (_fullParentPath is null)
            {
                throw new UnauthorizedAccessException();
            }

            await EnsureNoConflictingFileExistsAsync(cancellationToken).ConfigureAwait(false);

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
                .AsTask(cancellationToken)
                .WithConvertedException()
                .ConfigureAwait(false);
        }

        public override async Task<StorageFolder> CopyAsync(
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

            destinationPath = destinationPath.ToPhysicalStoragePath(FileSystem);
            if (destinationPath.FullPath.Parent is null)
            {
                throw new IOException(ExceptionStrings.StorageFolder.CannotMoveToRootLocation());
            }

            if (destinationPath.FullPath == _fullPath)
            {
                throw new IOException(ExceptionStrings.StorageFolder.CannotCopyToSameLocation());
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
                    .AsTask(cancellationToken)
                    .WithConvertedException()
                    .ConfigureAwait(false);

                foreach (var file in await src.GetFilesAsync().AsAwaitable(cancellationToken))
                {
                    await file
                        .CopyAsync(dstFolder, file.Name)
                        .AsTask(cancellationToken)
                        .WithConvertedException()
                        .ConfigureAwait(false);
                }

                foreach (var folder in await src.GetFoldersAsync().AsAwaitable(cancellationToken))
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
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            destinationPath = destinationPath.ToPhysicalStoragePath(FileSystem);

            var fullDestinationPath = destinationPath.FullPath;
            var destinationFolder = FileSystem.GetFolder(fullDestinationPath);

            // There is no native Move API. The current "best practice" (haha) is to simply copy
            // a folder instead of moving.
            // We might be able to improve performance if we're moving into the same directory, i.e.
            // if we're effectively doing a rename.
            // Since this is path based, we have to be careful, of course.
            if (fullDestinationPath.Parent == _fullParentPath)
            {
                await RenameAsync(destinationPath.FullPath.Name, options, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await CopyAsync(destinationPath, options, cancellationToken).ConfigureAwait(false);
            }

            await DeleteAsync(DeletionOption.IgnoreMissing).ConfigureAwait(false);
            return destinationFolder;
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
                throw new ArgumentException(
                    ExceptionStrings.StorageFolder.NewNameContainsInvalidChar(FileSystem.PathInformation),
                    nameof(newName)
                );
            }

            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            var srcFolder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var fullDestinationPath = _fullParentPath is null
                ? FileSystem.GetPath(newName).FullPath
                : _fullParentPath.Join(newName).FullPath;

            // The Windows Storage API doesn't do a hard replace with the ReplaceExisting option.
            // For example, if we had this structure:
            // |_ src
            // |  |_ foo.ext
            // |_ dst
            //    |_ bar.ext
            //
            // and renamed `src` to `dst`, we'd get this result:
            // |_ dst
            //    |_ foo.ext
            //    |_ bar.ext
            // 
            // What we (and the spec) want is this:
            // |_ dst
            //    |_ foo.ext
            //
            // We can manually delete the dst folder if it exists to fulfill the specification.
            // We're *only* doing it if we can be sure that we're not doing an in-place rename though,
            // i.e. rename `src` to `src`.
            // Otherwise we'd run into the problem that `src` is deleted and that the rename operation
            // will fail (DirectoryNotFound). Afterwards the entire folder is gone permanently.
            // That must be avoided at all cost.
            if (options == NameCollisionOption.ReplaceExisting &&
                !fullDestinationPath.Name.Equals(_fullPath.Name, FileSystem.PathInformation.DefaultStringComparison))
            {
                try
                {
                    var dstFolder = await FsHelper.GetFolderAsync(fullDestinationPath, cancellationToken).ConfigureAwait(false);
                    await dstFolder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsAwaitable(cancellationToken);
                }
                catch
                {
                    // If deleting the conflicting folder fails, it's okay, since the whole process
                    // is just there for fulfilling the specification.
                    // The Windows Storage API will still replace conflicting elements.
                    // It's just that certain files may be left over (as described above).
                    // Not fulfilling the spec is the best thing we can do without taking higher risks
                    // of lost data.
                }
            }

            await srcFolder
                .RenameAsync(newName, options.ToWinOptions())
                .AsTask(cancellationToken)
                .WithConvertedException()
                .ConfigureAwait(false);

            return FileSystem.GetFolder(fullDestinationPath);
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
                var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsAwaitable(cancellationToken);
            }

            async Task IgnoreMissingImpl()
            {
                try
                {
                    var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
                    if (folder is object)
                    {
                        await folder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsAwaitable(cancellationToken);
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
            var allFiles = await folder.GetFilesAsync().AsAwaitable(cancellationToken);
            return allFiles.Select(winStorageFolder => FileSystem.GetFile(winStorageFolder.Path));
        }

        public override async Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            var folder = await FsHelper.GetFolderAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            var allFolders = await folder.GetFoldersAsync().AsAwaitable(cancellationToken);
            return allFolders.Select(winStorageFolder => FileSystem.GetFolder(winStorageFolder.Path));
        }

        private async Task EnsureNoConflictingFileExistsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await FsHelper.GetFileAsync(_fullPath, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Ideally we'd catch more specific exceptions here.
                // Since this method is only called for throwing the *correct* exception type though,
                // we can be less strict about it.
                // At the end of the day, if a conflicting file does exist, the I/O APIs *will*
                // throw, just not a guaranteed IOException. No need to make our life harder with
                // extensive exception checking.
                return;
            }

            throw new IOException(ExceptionStrings.StorageFolder.ConflictingFileExistsAtFolderLocation());
        }
    }
}
