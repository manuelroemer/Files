namespace Files.Utilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Extends the <see cref="FileSystemElement"/> class with useful extension methods.
    /// </summary>
    public static class FileSystemElementExtensions
    {

        public static Folder? GetParent(this FileSystemElement element) => element switch
        {
            File file => file.GetParent(),
            Folder folder => folder.GetParent(),
            _ => throw new ArgumentNullException(nameof(element))
        };

        public static async Task<FileSystemElementProperties> GetPropertiesAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        ) => element switch
        {
            File file => await file.GetPropertiesAsync(cancellationToken).ConfigureAwait(false),
            Folder folder => await folder.GetPropertiesAsync(cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentNullException(nameof(element))
        };

        public static Task CreateOrIgnoreAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, options: CreationCollisionOption.Ignore, cancellationToken: cancellationToken);
        }

        public static Task CreateOrReplaceAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, options: CreationCollisionOption.ReplaceExisting, cancellationToken: cancellationToken);
        }

        public static Task CreateRecursivelyAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, options: CreationCollisionOption.Fail, cancellationToken: cancellationToken);
        }

        public static Task CreateOrIgnoreRecursivelyAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, options: CreationCollisionOption.Ignore, cancellationToken: cancellationToken);
        }

        public static Task CreateOrReplaceRecursivelyAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, options: CreationCollisionOption.ReplaceExisting, cancellationToken: cancellationToken);
        }

        public static Task DeleteOrIgnoreMissingAsync(
            this FileSystemElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken);
        }

    }

}
