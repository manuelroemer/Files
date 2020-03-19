namespace Files.FileSystems.Physical.Utilities
{
    using System.IO;
    using System.Threading;

    internal static class DirectoryHelper
    {

        public static void CopyDirectory(string source, string destination, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var directories = Directory.GetDirectories(source);
            var files = Directory.GetFiles(source);

            Directory.CreateDirectory(destination);

            foreach (var dirSrc in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dirDest = Path.Join(destination, Path.GetFileName(dirSrc));
                CopyDirectory(dirSrc, dirDest, cancellationToken);
            }

            foreach (var fileSrc in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileDest = Path.Join(destination, Path.GetFileName(fileSrc));
                File.Copy(fileSrc, fileDest);
            }
        }

    }

}
