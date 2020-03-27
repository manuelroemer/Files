namespace Files.Utilities
{
    using System;
    using System.Threading.Tasks;
    using System.IO;
    using System.Threading;
    using StorageFile = Files.StorageFile;

    public static class FileExtensions
    {
        public static Task<Stream> OpenReadAsync(this StorageFile file, CancellationToken cancellationToken = default)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));
            return file.OpenAsync(FileAccess.Read, cancellationToken);
        }

        public static Task<Stream> OpenWriteAsync(this StorageFile file, CancellationToken cancellationToken = default)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));
            return file.OpenAsync(FileAccess.Write, cancellationToken);
        }
    }
}
