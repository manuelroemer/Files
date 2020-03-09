namespace Files.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using System.IO;
    using System.Threading;
    using File = Files.File;

    public static class FileExtensions
    {

        public static Task<Stream> OpenReadAsync(this File file, CancellationToken cancellationToken = default)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));
            return file.OpenAsync(FileAccess.Read, cancellationToken);
        }

        public static Task<Stream> OpenWriteAsync(this File file, CancellationToken cancellationToken = default)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));
            return file.OpenAsync(FileAccess.Write, cancellationToken);
        }

    }

}
