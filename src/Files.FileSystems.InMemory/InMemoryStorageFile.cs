#pragma warning disable CS1998
// Async method lacks 'await' operators and will run synchronously.
// The entire InMemoryFileSystem implementation is synchronous. Nontheless, exceptions should be
// propagated via Task instances.

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
    using Files.FileSystems.InMemory.FsTree;
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

        public override async Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
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

                return properties;
            }
        }

        public override async Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return _storage.GetFileNode(Path).Attributes;
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
                _storage.GetFileNode(Path).Attributes = attributes;
            }
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                return _storage.HasFileNode(Path);
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
                if (recursive)
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

        public override async Task<StorageFile> CopyAsync(
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
                var fileNode = _storage.GetFileNode(Path);
                fileNode.Copy(destinationPath, replaceExisting);
                return FileSystem.GetFile(destinationPath.FullPath);
            }
        }

        public override async Task<StorageFile> MoveAsync(
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

        private StorageFile MoveInternalNotLocking(
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

            lock (_inMemoryFileSystem.Storage)
            {
                var fileNode = _storage.GetFileNode(Path);
                fileNode.Move(destinationPath, replaceExisting);
                return FileSystem.GetFile(fileNode.Path.FullPath);
            }
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

            cancellationToken.ThrowIfCancellationRequested();

            lock (_inMemoryFileSystem.Storage)
            {
                var destinationPath = Parent.Path.FullPath.Join(newName);
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
                        _storage.GetFileNode(Path).Delete();
                        break;
                    case DeletionOption.IgnoreMissing:
                        _storage.TryGetFileNodeAndThrowOnConflictingFolder(Path)?.Delete();
                        break;
                    default:
                        throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options));
                }
            }
        }


        //
        // The Open.../Read.../Write... methods below do not require explicit locking.
        // Locking only happens when acquiring the FileContentStream from the node (via OpenFileContentStream(...)).
        // Once the stream exists, the underlying FileNode automatically protects itself against
        // modification since the file is "in use".
        // See FileContentStream/FileNode for details.
        //

        public override async Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(fileAccess))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(fileAccess), nameof(fileAccess));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return OpenFileContentStream(fileAccess);
        }

        public override async Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenFileContentStream(FileAccess.Read);
            return stream.ToArray();
        }

        public override async Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenFileContentStream(FileAccess.Write, replaceExistingContent: true);
            stream.Write(bytes, 0, bytes.Length);
        }

        public override async Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            encoding ??= _inMemoryFileSystem.Options.DefaultEncoding;
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenFileContentStream(FileAccess.Read);
            using var reader = new StreamReader(stream, encoding);
            return reader.ReadToEnd();
        }

        public override async Task WriteTextAsync(string text, Encoding? encoding, CancellationToken cancellationToken = default)
        {
            _ = text ?? throw new ArgumentNullException(nameof(text));
            encoding ??= _inMemoryFileSystem.Options.DefaultEncoding;
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = OpenFileContentStream(FileAccess.Write, replaceExistingContent: true);
            using var writer = new StreamWriter(stream, encoding);
            writer.Write(text);
        }

        private FileContentStream OpenFileContentStream(FileAccess fileAccess, bool replaceExistingContent = false)
        {
            lock (_inMemoryFileSystem.Storage)
            {
                var fileNode = _storage.GetFileNode(Path);
                return fileNode.Content.Open(fileAccess, replaceExistingContent);
            }
        }
    }
}

#pragma warning restore CS1998
