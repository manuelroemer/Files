﻿namespace Files.FileSystems.Physical
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.Physical.Utilities;
    using Files.Shared;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using IOPath = System.IO.Path;

    internal sealed class PhysicalStorageFolder : StorageFolder
    {
        private readonly StoragePath _fullPath;
        private readonly StoragePath? _fullParentPath;

        public PhysicalStorageFolder(PhysicalFileSystem fileSystem, PhysicalStoragePath path)
            : base(fileSystem, path)
        {
            _fullPath = path.FullPath;
            _fullParentPath = path.FullPath.Parent;
        }

        public override Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                EnsureExists();

                // Attempting to get the real folder name can fail, e.g. the folder might have been deleted in between.
                // In such a case, simply return the last fetched name. It will happen rarely and is good enough
                // for such cases.
                cancellationToken.ThrowIfCancellationRequested();
                var realFolderName = FsHelper.GetRealDirectoryName(_fullPath.ToString()) ?? Path.Name;
                var lastWriteTime = Directory.GetLastWriteTimeUtc(_fullPath.ToString());

                return new StorageFolderProperties(
                    realFolderName,
                    IOPath.GetFileNameWithoutExtension(realFolderName),
                    PhysicalPathHelper.GetExtensionWithoutTrailingExtensionSeparator(realFolderName)?.ToNullIfEmpty(),
                    Directory.GetCreationTimeUtc(_fullPath.ToString()),
                    lastWriteTime
                );
            }, cancellationToken);
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    EnsureNoConflictingFileExists();
                    cancellationToken.ThrowIfCancellationRequested();
                    return File.GetAttributes(_fullPath.ToString());
                }
                catch (FileNotFoundException ex)
                {
                    // Since we're using a File API, we must manually convert the FileNotFoundException.
                    throw new DirectoryNotFoundException(message: null, ex);
                }
            }, cancellationToken);
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(attributes))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(attributes), nameof(attributes));
            }

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
            }, cancellationToken);
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Directory.Exists(_fullPath.ToString()), cancellationToken);
        }

        public override Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            return Task.Run(async () =>
            {
                // Directory.CreateDirectory is recursive by default.
                // If recursive is false, we must manually ensure that the parent directory exists (if this is not
                // a root directory).
                if (!recursive && _fullParentPath is not null && !Directory.Exists(_fullParentPath.ToString()))
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
                            throw new IOException(ExceptionStrings.StorageFolder.CreateFailFolderAlreadyExists());
                        case CreationCollisionOption.ReplaceExisting:
                            await DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken).ConfigureAwait(false);
                            break;
                        case CreationCollisionOption.UseExisting:
                            return;
                        default:
                            throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options));
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(_fullPath.ToString());
            }, cancellationToken);
        }

        public override Task<StorageFolder> CopyAsync(
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

            return Task.Run(() =>
            {
                var fullDestinationPath = destinationPath.FullPath;
                var destination = FileSystem.GetFolder(destinationPath);
                var overwrite = options.ToOverwriteBool();

                // Specification requires DirectoryNotFoundException if the destination parent folder
                // does not exist.
                if (destination.Parent is PhysicalStorageFolder destinationParent)
                {
                    destinationParent.EnsureExists();
                }

                if (overwrite)
                {
                    DeleteConflictingDestinationFolderWithoutDeletingThisFolder(fullDestinationPath.ToString());

                    // At this point the conflicting destination folder should be deleted.
                    // If that is not the case, we can assume that we are essentially copying
                    // the folder to the same location (because otherwise, the method above would
                    // have deleted the folder at the destination path).
                    if (Directory.Exists(fullDestinationPath.ToString()))
                    {
                        throw new IOException(ExceptionStrings.StorageFolder.CannotCopyToSameLocation());
                    }
                }
                else
                {
                    // The CopyDirectory helper cannot easily verify that no conflicting folder exists
                    // at the destination. Specification requires an IOException on conflicts for the Fail option.
                    if (Directory.Exists(fullDestinationPath.ToString()))
                    {
                        throw new IOException(ExceptionStrings.StorageFolder.CopyConflictingFolderExistsAtDestination());
                    }
                }

                FsHelper.CopyDirectory(_fullPath.ToString(), fullDestinationPath.ToString(), cancellationToken);
                return destination;
            }, cancellationToken);
        }

        public override Task<StorageFolder> MoveAsync(
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

            return Task.Run(() =>
            {
                // For whatever reason, Directory.Move moves files instead of throwing an exception.
                // We've got to manually verify that the current location actually is a directory and not a file.
                EnsureNoConflictingFileExists();

                var fullDestinationPath = destinationPath.FullPath;
                var destination = FileSystem.GetFolder(destinationPath);
                var overwrite = options.ToOverwriteBool();

                if (overwrite)
                {
                    DeleteConflictingDestinationFolderWithoutDeletingThisFolder(fullDestinationPath.ToString());

                    // At this point the conflicting destination folder should be deleted.
                    // If that is not the case, we can assume that we are essentially moving
                    // the folder to the same location (because otherwise, the method above would
                    // have deleted the folder at the destination path).
                    if (Directory.Exists(fullDestinationPath.ToString()))
                    {
                        throw new IOException(ExceptionStrings.StorageFolder.CannotMoveToSameLocation());
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.Move(_fullPath.ToString(), fullDestinationPath.ToString());
                return destination;
            }, cancellationToken);
        }

        private void DeleteConflictingDestinationFolderWithoutDeletingThisFolder(string fullDestinationPath)
        {
            // Both Move and Copy require manual deletion of a conflicting folder at the destination
            // with the ReplaceExisting option.
            // This can be a problem if we want to move/copy a folder to the same location,
            // because we'd delete the destination folder which is *also the source folder*.
            // In essence, the folder would be gone for good.
            // We prevent this with a little hack: Temporarily renaming the source folder
            // so that deleting the destination does not automatically delete the source.
            // After the deletion, we undo the rename and can continue with the actual moving/copying.
            var tmpFolderName = PhysicalPathHelper.GetTemporaryElementName();
            var tmpFolderPath = IOPath.Combine(_fullParentPath?.ToString() ?? "", tmpFolderName);

            Directory.Move(_fullPath.ToString(), tmpFolderPath);

            try
            {
                Directory.Delete(fullDestinationPath, recursive: true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            finally
            {
                Directory.Move(tmpFolderPath, _fullPath.ToString());
            }
        }

        public override Task<StorageFolder> RenameAsync(
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

            var destinationPath = _fullParentPath?.Join(newName) ?? FileSystem.GetPath(newName);
            return MoveAsync(destinationPath, options, cancellationToken);
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            return Task.Run(
                options switch
                {
                    DeletionOption.Fail => FailImpl,
                    DeletionOption.IgnoreMissing => IgnoreMissingImpl,
                    _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
                },
                cancellationToken
            );
            
            void FailImpl()
            {
                EnsureNoConflictingFileExists(); // Unix doesn't throw when a file exists at the same location.

                cancellationToken.ThrowIfCancellationRequested();
                EnsureExists();
                
                cancellationToken.ThrowIfCancellationRequested();
                Directory.Delete(_fullPath.ToString(), recursive: true);
            }

            void IgnoreMissingImpl()
            {
                EnsureNoConflictingFileExists(); // Unix doesn't throw when a file exists at the same location.

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Directory.Delete(_fullPath.ToString(), recursive: true);
                }
                catch (DirectoryNotFoundException) { }
            }
        }

        public override Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                // Use GetFiles() instead of EnumerateFiles() for two reasons:
                // - We're inside of a Task.Run. The retrieval should run/finish on that task.
                // - GetFiles immediately throws. The Enumerate version delays until the first enumeration.
                return Directory
                    .GetFiles(_fullPath.ToString())
                    .Select(path => FileSystem.GetFile(path));
            }, cancellationToken);
        }

        public override Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                // Use GetDirectories() instead of EnumerateDirectories() for two reasons:
                // - We're inside of a Task.Run. The retrieval should run/finish on that task.
                // - GetDirectories immediately throws. The Enumerate version delays until the first enumeration.
                return Directory
                    .GetDirectories(_fullPath.ToString())
                    .Select(path => FileSystem.GetFolder(path));
            }, cancellationToken);
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
                throw new IOException(ExceptionStrings.StorageFolder.ConflictingFileExistsAtFolderLocation());
            }
        }
    }
}
