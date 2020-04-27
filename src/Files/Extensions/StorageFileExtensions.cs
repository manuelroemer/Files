namespace Files.Extensions
{
    using System;
    using System.Threading.Tasks;
    using System.IO;
    using System.Threading;

    public static class StorageFileExtensions
    {
        public static Task<StorageFile> CopyOrReplaceAsync(
            this StorageFile storageFile,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.CopyAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task<StorageFile> MoveOrReplaceAsync(
            this StorageFile storageFile,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.MoveAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task<StorageFile> RenameOrReplaceAsync(
            this StorageFile storageFile,
            string newName,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.RenameAsync(newName, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task<Stream> OpenReadAsync(
            this StorageFile storageFile,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.OpenAsync(FileAccess.Read, cancellationToken);
        }

        public static Task<Stream> OpenWriteAsync(
            this StorageFile storageFile,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.OpenAsync(FileAccess.Write, cancellationToken);
        }
    }
}
