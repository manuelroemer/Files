namespace Files.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class StorageElementExtensions
    {
        public static Task CreateOrIgnoreAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, CreationCollisionOption.Ignore, cancellationToken);
        }

        public static Task CreateOrReplaceAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, CreationCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task CreateRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.Fail, cancellationToken);
        }

        public static Task CreateOrIgnoreRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.Ignore, cancellationToken);
        }

        public static Task CreateOrReplaceRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task DeleteOrIgnoreMissingAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken);
        }
    }
}
