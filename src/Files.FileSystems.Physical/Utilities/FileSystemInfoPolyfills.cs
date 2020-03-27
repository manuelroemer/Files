namespace Files.FileSystems.Physical.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using IOPath = System.IO.Path;

    internal static class FileSystemInfoPolyfills
    {
        public static void MoveTo(this DirectoryInfo directoryInfo, string destination, bool overwrite)
        {
            ExceptionDispatchInfo? originalException = null;
            string? tmpDirName = null;

            if (overwrite)
            {
                var tmpName = IOPath.Join(destination, Guid.NewGuid().ToString(), ".tmp");
                if (Directory.Exists(destination))
                {
                    tmpDirName = tmpName;
                    Directory.Move(destination, tmpDirName);
                }
            }

            try
            {
                directoryInfo.MoveTo(destination);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031
            {
                originalException = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                var requiresRestore = originalException != null;
                if (requiresRestore)
                {
                    if (tmpDirName != null)
                    {
                        Directory.Move(tmpDirName, destination);
                    }

                    originalException!.Throw();
                }
                else
                {
                    if (tmpDirName != null)
                    {
                        Directory.Delete(tmpDirName, recursive: true);
                    }
                }
            }
        }

#if !HAS_MOVE_OVERWRITE_OVERLOAD

        public static void MoveTo(this FileInfo fileInfo, string destination, bool overwrite)
        {
            if (overwrite)
            {
                File.Delete(destination);
            }
            fileInfo.MoveTo(destination);
        }
        
#endif
    }
}
