namespace Files.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class StorageElementExtensions
    {
        public static Task CreateOrReplaceExistingAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, CreationCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task CreateOrUseExistingAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, CreationCollisionOption.UseExisting, cancellationToken);
        }

        public static Task CreateRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.Fail, cancellationToken);
        }

        public static Task CreateOrReplaceExistingRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting, cancellationToken);
        }

        public static Task CreateOrUseExistingRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.UseExisting, cancellationToken);
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
