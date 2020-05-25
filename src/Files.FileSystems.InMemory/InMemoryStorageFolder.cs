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
    using Files.FileSystems.InMemory.Internal;
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

        public override Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            var node = _storage.GetFolderNode(Path);
            var properties = new StorageFolderProperties(
                name: node.Path.Name,
                nameWithoutExtension: node.Path.NameWithoutExtension,
                extension: node.Path.Extension,
                createdOn: node.CreatedOn,
                modifiedOn: node.ModifiedOn
            );

            return Task.FromResult(properties);
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.GetFolderNode(Path).Attributes);
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(attributes))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(attributes), nameof(attributes));
            }

            _storage.GetFolderNode(Path).Attributes = attributes;
            return Task.CompletedTask;
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            var exists = _storage.HasFolderNode(Path);
            return Task.FromResult(exists);
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

            // Recursively call this method until all parent folders exist (if required).
            // It's enough to check whether an ElementNode exists because the storage will throw on
            // conflicting Node types later.
            if (recursive && Parent is object && !_storage.HasElementNode(Parent.Path))
            {
                await Parent
                    .CreateAsync(recursive: true, CreationCollisionOption.UseExisting, cancellationToken)
                    .ConfigureAwait(false);
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

        public override Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
        }

        public override Task<StorageFolder> MoveAsync(
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

            var replaceExisting = options switch
            {
                NameCollisionOption.Fail => false,
                NameCollisionOption.ReplaceExisting => true,
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
            };

            var folderNode = _storage.GetFolderNode(Path);
            folderNode.Move(destinationPath, replaceExisting);
            return Task.FromResult(FileSystem.GetFolder(folderNode.Path.FullPath));
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

            var destinationPath = Path.FullPath.Parent?.Join(newName) ?? FileSystem.GetPath(newName);
            return MoveAsync(destinationPath, options, cancellationToken);
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

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

            return Task.CompletedTask;
        }

        public override Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            var files = _storage
                .GetFolderNode(Path)
                .Children
                .OfType<FileNode>()
                .Select(node => FileSystem.GetFile(node.Path));
            return Task.FromResult(files);
        }

        public override Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            var folders = _storage
                .GetFolderNode(Path)
                .Children
                .OfType<FolderNode>()
                .Select(node => FileSystem.GetFolder(node.Path));
            return Task.FromResult(folders);
        }
    }
}
