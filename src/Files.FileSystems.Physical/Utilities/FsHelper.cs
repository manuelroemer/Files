namespace Files.FileSystems.Physical.Utilities
{
    using System.IO;
    using System.Threading;
    using Files.Shared.PhysicalStoragePath.Utilities;

    internal static class FsHelper
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
                var dirDest = PathPolyfills.Join(destination, Path.GetFileName(dirSrc));
                CopyDirectory(dirSrc, dirDest, cancellationToken);
            }

            foreach (var fileSrc in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileDest = PathPolyfills.Join(destination, Path.GetFileName(fileSrc));
                File.Copy(fileSrc, fileDest);
            }
        }

        public static string? GetRealFileName(string fullPath)
        {
            var parentPath = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);
            var realChildren = Directory.GetFiles(parentPath, searchPattern: fileName);

            if (realChildren.Length != 1)
            {
                // There is either no child with this name or too many to make a pick.
                return null;
            }
            else
            {
                return Path.GetFileName(realChildren[0]);
            }
        }

        public static string? GetRealDirectoryName(string fullPath)
        {
            var parentPath = Path.GetDirectoryName(fullPath);
            var directoryName = Path.GetFileName(fullPath);
            var realChildren = Directory.GetDirectories(parentPath, searchPattern: directoryName);

            if (realChildren.Length != 1)
            {
                // There is either no child with this name or too many to make a pick.
                return null;
            }
            else
            {
                return Path.GetFileName(realChildren[0]);
            }
        }
    }
}
