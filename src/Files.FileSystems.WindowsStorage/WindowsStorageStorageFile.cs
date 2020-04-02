namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files.FileSystems.WindowsStorage.Resources;
    using Files.Shared.PhysicalStoragePath;
    using WinStorageFile = Windows.Storage.StorageFile;
    using WinStorageFolder = Windows.Storage.StorageFolder;

    internal sealed class WindowsStorageStorageFile : StorageFile
    {
        public override FileSystem FileSystem { get; }

        public override StoragePath Path => throw new NotImplementedException();

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
        }

        public override Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
        }

        public override Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            throw new NotImplementedException();
        }

        public override Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            throw new NotImplementedException();
        }

        public override Task<StorageFile> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));
            throw new NotImplementedException();
        }

        public override Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
        }
    }
}
