namespace Files.FileSystems.Physical
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.Physical.Resources;
    using Files.FileSystems.Physical.Utilities;
    using Files.Utilities;
    using IOPath = System.IO.Path;

    internal sealed class PhysicalFolder : StorageFolder
    {

        private readonly StoragePath _path;
        private readonly StoragePath _fullPath;
        private readonly StoragePath? _fullParentPath;
        private readonly DirectoryInfo _directoryInfo;

        public override FileSystem FileSystem { get; }

        public override StoragePath Path => _path;

        public PhysicalFolder(FileSystem fileSystem, PhysicalStoragePath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            FileSystem = fileSystem;
            _path = path;
            _fullPath = path.FullPath;
            _fullParentPath = path.FullPath.Parent;
            _directoryInfo = new DirectoryInfo(_fullPath);
        }

        public override Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                _directoryInfo.Refresh();
                EnsureExists();

                // Attempting to get the real folder name can fail, e.g. the folder might have been deleted in between.
                // In such a case, simply return the last fetched name. It will happen rarely and is good enough
                // for such cases.
                cancellationToken.ThrowIfCancellationRequested();
                var realFolderName = _directoryInfo.GetRealName() ?? _directoryInfo.Name;
                var lastWriteTime = Directory.GetLastWriteTimeUtc(_fullPath.ToString());

                return new StorageFolderProperties(
                    realFolderName,
                    IOPath.GetFileNameWithoutExtension(realFolderName),
                    PathHelper.GetExtensionWithoutTrailingExtensionSeparator(realFolderName)?.ToNullIfEmpty(),
                    _directoryInfo.CreationTimeUtc,
                    lastWriteTime
                );
            });
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                EnsureNoConflictingFileExists();
                cancellationToken.ThrowIfCancellationRequested();
                return File.GetAttributes(_fullPath.ToString());
            });
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                try
                {
                    EnsureNoConflictingFileExists();
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

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() => Directory.Exists(_fullPath.ToString()));
        }

        public override Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                // Directory.CreateDirectory is recursive by default.
                // If recursive is false, we must manually ensure that the parent directory exists (if this is not
                // a root directory).
                if (!recursive && _fullParentPath is object && !Directory.Exists(_fullParentPath.ToString()))
                {
                    throw new DirectoryNotFoundException();
                }

                // Directory.CreateDirectory is very user friendly in the sense that it rarely throws exceptions.
                // This means that we have to manually implement support for collision options.
                // This can lead to race conditions, but it's the best we can do.
                if (await ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    switch (options)
                    {
                        case CreationCollisionOption.Fail:
                            throw new IOException(ExceptionStrings.Folder.CreateFailFolderAlreadyExists());
                        case CreationCollisionOption.ReplaceExisting:
                            await DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken).ConfigureAwait(false);
                            break;
                        case CreationCollisionOption.Ignore:
                            return;
                        default:
                            throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options));
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(_fullPath.ToString());
            });
        }

        public override Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(async () =>
            {
                var destination = FileSystem.GetFolder(destinationPath);

                // Specification requires DirectoryNotFoundException if the destination parent folder
                // does not exist,
                if (destination.GetParent() is PhysicalFolder destinationParent)
                {
                    destinationParent.EnsureExists();
                }

                // The case of an existing folder at destination must also be manually handled.
                // The CopyDirectory utility function cannot easily do that using System.IO members,
                // because methods like Directory.CreateDirectory() are recursive by default.
                if (await destination.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    var overwrite = options.ToOverwriteBool();
                    if (overwrite)
                    {
                        await destination.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new IOException(ExceptionStrings.Folder.CopyConflictingFolderExistsAtDestination());
                    }
                }

                DirectoryHelper.CopyDirectory(_fullPath.ToString(), destinationPath.FullPath.ToString(), cancellationToken);
                return destination;
            });
        }

        public override Task<StorageFolder> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(async () =>
            {
                var destination = FileSystem.GetFolder(destinationPath);
                var overwrite = options.ToOverwriteBool();

                if (overwrite && await destination.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    await destination.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken).ConfigureAwait(false);
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.Move(_fullPath.ToString(), destinationPath.FullPath.ToString());
                return destination;
            });
        }

        public override Task<StorageFolder> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            cancellationToken.ThrowIfCancellationRequested();
            var destinationPath = _fullParentPath?.Join(newName) ?? FileSystem.GetPath(newName);
            return MoveAsync(destinationPath, options, cancellationToken);
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                if (options == DeletionOption.Fail)
                {
                    EnsureExists();
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Directory.Delete(_fullPath.ToString(), recursive: true);
                }
                catch (DirectoryNotFoundException)
                {
                    // The exception is thrown if a parent directory does not exist.
                    // Must be caught manually to ensure compatibility with DeletionOption.IgnoreMissing.
                    if (options != DeletionOption.IgnoreMissing)
                    {
                        throw;
                    }
                }
            });
        }

        public override Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                return Directory
                    .GetFiles(_fullPath.ToString())
                    .Select(path => FileSystem.GetFile(path));
            });
        }

        public override Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                return Directory
                    .GetDirectories(_fullPath.ToString())
                    .Select(path => FileSystem.GetFolder(path));
            });
        }

        private void EnsureExists()
        {
            if (!Directory.Exists(_fullPath.ToString()))
            {
                throw new DirectoryNotFoundException();
            }
        }

        private void EnsureNoConflictingFileExists()
        {
            if (File.Exists(_fullPath.ToString()))
            {
                throw new IOException(ExceptionStrings.Folder.ConflictingFileExistsAtFolderLocation());
            }
        }

    }

}
