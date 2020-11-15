#pragma warning disable CS1998
// Async method lacks 'await' operators and will run synchronously.
// The entire InMemoryFileSystem implementation is synchronous. Nontheless, exceptions should be
// propagated via Task instances.

namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Files;
    using System.Diagnostics;
    using System.Threading;
    using System.IO;
    using Files.FileSystems.InMemory.FsTree;
    using Files.Shared;

    internal sealed class InMemoryStorageFolder : StorageFolder
    {
        private readonly InMemoryFileSystem _inMemoryFileSystem;
        private readonly FsDataStorage _storage;

        public InMemoryStorageFolder(InMemoryFileSystem fileSystem, StoragePath path)
            : base(fileSystem, path)
        {
            Debug.Assert(
                ReferenceEquals(fileSystem, path.FileSystem),
                "The InMemoryFileSystem should ensure that the InMemoryStoragePath was created by itself."
            );

            _inMemoryFileSystem = fileSystem;
            _storage = fileSystem.Storage;
        }

        public override async Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                var node = _storage.GetFolderNode(Path);
                var properties = new StorageFolderProperties(
                    name: node.Path.Name,
                    nameWithoutExtension: node.Path.NameWithoutExtension,
                    extension: node.Path.Extension,
                    createdOn: node.CreatedOn,
                    modifiedOn: node.ModifiedOn
                );

                return properties;
            }
        }

        public override async Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return _storage.GetFolderNode(Path).Attributes;
            }
        }

        public override async Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(attributes))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(attributes), nameof(attributes));
            }

            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                _storage.GetFolderNode(Path).Attributes = attributes;
            }
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return _storage.HasFolderNode(Path);
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

            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                CreateInternalNotLocking(recursive, options, cancellationToken);
            }
        }

        /// <summary>
        ///     The actual Create implementation.
        ///     Extracted since this method must be callable recursively by itself and InMemoryStorageFile.
        ///     Therefore, no lock must be aquired.
        /// </summary>
        internal void CreateInternalNotLocking(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Recursively call this method until all parent folders exist (if required).
            // It's enough to check whether an ElementNode exists because the storage will throw on
            // conflicting Node types later.
            if (recursive && Parent is not null && !_storage.HasElementNode(Parent.Path))
            {
                var inMemoryParent = (InMemoryStorageFolder)Parent;
                inMemoryParent.CreateInternalNotLocking(
                    recursive: true,
                    CreationCollisionOption.UseExisting,
                    cancellationToken
                );
            }

            switch (options)
            {
                case CreationCollisionOption.Fail:
                    FailImpl();
                    break;
                case CreationCollisionOption.ReplaceExisting:
                    ReplaceExistingImpl();
                    break;
                case CreationCollisionOption.UseExisting:
                    UseExistingImpl();
                    break;
                default:
                    throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options));
            }

            void FailImpl()
            {
                FolderNode.Create(_storage, Path);
            }

            void ReplaceExistingImpl()
            {
                if (_storage.TryGetFolderNode(Path) is FolderNode existingNode)
                {
                    existingNode.Delete();
                }
                FolderNode.Create(_storage, Path);
            }

            void UseExistingImpl()
            {
                if (!_storage.HasFolderNode(Path))
                {
                    FolderNode.Create(_storage, Path);
                }
            }
        }

        public override async Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));

            if (!ReferenceEquals(destinationPath.FileSystem, FileSystem))
            {
                throw new ArgumentException(
                    ExceptionStrings.InMemoryFileSystem.MemberIncompatibleWithInstance(),
                    nameof(destinationPath)
                );
            }

            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var replaceExisting = options switch
            {
                NameCollisionOption.Fail => false,
                NameCollisionOption.ReplaceExisting => true,
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
            };

            lock (_inMemoryFileSystem.Storage)
            {
                var folderNode = _storage.GetFolderNode(Path);
                folderNode.Copy(destinationPath, replaceExisting);
                return FileSystem.GetFolder(destinationPath.FullPath);
            }
        }

        public override async Task<StorageFolder> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));

            if (!ReferenceEquals(destinationPath.FileSystem, FileSystem))
            {
                throw new ArgumentException(
                    ExceptionStrings.InMemoryFileSystem.MemberIncompatibleWithInstance(),
                    nameof(destinationPath)
                );
            }

            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return MoveInternalNotLocking(destinationPath, options, cancellationToken);
            }
        }

        private StorageFolder MoveInternalNotLocking(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var replaceExisting = options switch
            {
                NameCollisionOption.Fail => false,
                NameCollisionOption.ReplaceExisting => true,
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
            };
            
            var folderNode = _storage.GetFolderNode(Path);
            folderNode.Move(destinationPath, replaceExisting);
            return FileSystem.GetFolder(folderNode.Path.FullPath);
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

            if (newName.Contains(_inMemoryFileSystem.InvalidNewNameCharacters))
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

            cancellationToken.ThrowIfCancellationRequested();

            var destinationPath = Path.FullPath.Parent?.Join(newName) ?? FileSystem.GetPath(newName);

            lock (_inMemoryFileSystem.Storage)
            {
                return MoveInternalNotLocking(destinationPath, options, cancellationToken);
            }
        }

        public override async Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                switch (options)
                {
                    case DeletionOption.Fail:
                        _storage.GetFolderNode(Path).Delete();
                        break;
                    case DeletionOption.IgnoreMissing:
                        _storage.TryGetFolderNodeAndThrowOnConflictingFile(Path)?.Delete();
                        break;
                    default:
                        throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options));
                }
            }
        }

        public override async Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return _storage
                    .GetFolderNode(Path)
                    .Children
                    .OfType<FileNode>()
                    .Select(node => FileSystem.GetFile(node.Path));
            }
        }

        public override async Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return _storage
                    .GetFolderNode(Path)
                    .Children
                    .OfType<FolderNode>()
                    .Select(node => FileSystem.GetFolder(node.Path));
            }
        }
    }
}

#pragma warning restore CS1998
