namespace Files.FileSystems.InMemory
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.InMemory.Internal;
    using Files.Shared;

    internal sealed class InMemoryStorageFile : StorageFile
    {
        private readonly InMemoryFileSystem _inMemoryFileSystem;
        private readonly FsDataStorage _storage;

        public InMemoryStorageFile(InMemoryFileSystem fileSystem, StoragePath path)
            : base(fileSystem, path)
        {
            Debug.Assert(
                ReferenceEquals(fileSystem, path.FileSystem),
                "The InMemoryFileSystem should ensure that the InMemoryStoragePath was created by itself."
            );

            _inMemoryFileSystem = fileSystem;
            _storage = fileSystem.Storage;
        }

        public override Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            var node = _storage.GetFileNode(Path);
            var properties = new StorageFileProperties(
                name: node.Path.Name,
                nameWithoutExtension: node.Path.NameWithoutExtension,
                extension: node.Path.Extension,
                createdOn: node.CreatedOn,
                modifiedOn: node.ModifiedOn,
                size: node.Content.Size
            );

            return Task.FromResult(properties);
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.GetFileNode(Path).Attributes);
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            _storage.GetFileNode(Path).Attributes = attributes;
            return Task.CompletedTask;
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            var exists = _storage.HasFileNode(Path);
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

            if (recursive)
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
                FileNode.Create(_storage, Path);
            }

            void ReplaceExistingImpl()
            {
                if (_storage.TryGetFileNode(Path) is FileNode existingNode)
                {
                    existingNode.Delete();
                }
                FileNode.Create(_storage, Path);
            }

            void UseExistingImpl()
            {
                if (!_storage.HasFileNode(Path))
                {
                    FileNode.Create(_storage, Path);
                }
            }
        }

        public override Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
        }

        public override Task<StorageFile> MoveAsync(
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

            var fileNode = _storage.GetFileNode(Path);
            fileNode.Move(destinationPath);
            return Task.FromResult(FileSystem.GetFile(fileNode.Path));
        }

        public override Task<StorageFile> RenameAsync(
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
                    ExceptionStrings.StorageFile.NewNameContainsInvalidChar(FileSystem.PathInformation),
                    nameof(newName)
                );
            }

            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            var destinationPath = Parent.Path.FullPath.Join(newName);
            return MoveAsync(destinationPath, options, cancellationToken);
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            _storage.GetFileNode(Path).Delete();
            return Task.CompletedTask;
        }

        public override Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(OpenFileContentStream(fileAccess));
        }

        private FileContentStream OpenFileContentStream(FileAccess fileAccess)
        {
            var fileNode = _storage.GetFileNode(Path);
            return fileNode.Content.Open(fileAccess);
        }

        public override Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            using var stream = OpenFileContentStream(FileAccess.Read);
            return Task.FromResult(stream.ToArray());
        }

        public override Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            using var stream = OpenFileContentStream(FileAccess.Write);
            stream.Write(bytes, 0, bytes.Length);
            return Task.CompletedTask;
        }

        public override Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            encoding ??= _inMemoryFileSystem.Options.DefaultEncoding;
            using var stream = OpenFileContentStream(FileAccess.Read);
            using var reader = new StreamReader(stream, encoding);
            return Task.FromResult(reader.ReadToEnd());
        }

        public override Task WriteTextAsync(string text, Encoding? encoding, CancellationToken cancellationToken = default)
        {
            encoding ??= _inMemoryFileSystem.Options.DefaultEncoding;
            using var stream = OpenFileContentStream(FileAccess.Write);
            using var writer = new StreamWriter(stream, encoding);
            writer.Write(text);
            return Task.CompletedTask;
        }
    }
}
