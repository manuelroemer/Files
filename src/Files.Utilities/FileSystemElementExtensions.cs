namespace Files.Utilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Extends the <see cref="StorageElement"/> class with useful extension methods.
    /// </summary>
    public static class FileSystemElementExtensions
    {

        public static StorageFolder? GetParent(this StorageElement element) => element switch
        {
            StorageFile file => file.GetParent(),
            StorageFolder folder => folder.GetParent(),
            _ => throw new ArgumentNullException(nameof(element))
        };

        public static async Task<StorageElementProperties> GetPropertiesAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        ) => element switch
        {
            StorageFile file => await file.GetPropertiesAsync(cancellationToken).ConfigureAwait(false),
            StorageFolder folder => await folder.GetPropertiesAsync(cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentNullException(nameof(element))
        };

        public static Task CreateOrIgnoreAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, options: CreationCollisionOption.Ignore, cancellationToken: cancellationToken);
        }

        public static Task CreateOrReplaceAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, options: CreationCollisionOption.ReplaceExisting, cancellationToken: cancellationToken);
        }

        public static Task CreateRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, options: CreationCollisionOption.Fail, cancellationToken: cancellationToken);
        }

        public static Task CreateOrIgnoreRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, options: CreationCollisionOption.Ignore, cancellationToken: cancellationToken);
        }

        public static Task CreateOrReplaceRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, options: CreationCollisionOption.ReplaceExisting, cancellationToken: cancellationToken);
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
