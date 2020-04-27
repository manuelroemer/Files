namespace Files.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class StorageFolderExtensions
    {
        public static Task<StorageFolder> CopyOrReplaceAsync(
            this StorageFolder storageFolder,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));
            return storageFolder.CopyAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task<StorageFolder> MoveOrReplaceAsync(
            this StorageFolder storageFolder,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));
            return storageFolder.MoveAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task<StorageFolder> RenameOrReplaceAsync(
            this StorageFolder storageFolder,
            string newName,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));
            return storageFolder.RenameAsync(newName, NameCollisionOption.ReplaceExisting, cancellationToken);
        }
    }
}
