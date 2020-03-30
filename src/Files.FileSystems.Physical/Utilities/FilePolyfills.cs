namespace Files.FileSystems.Physical.Utilities
{
    using System.IO;

    internal static class FilePolyfills
    {
#if NETSTANDARD2_1 || NETCOREAPP2_2
        internal static void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            if (overwrite)
            {
                try
                {
                    // We are mocking overwrite support by simply deleting any existing destination file.
                    // But caution:
                    // If sourceFileName and destFileName point to the same file, we would, by default,
                    // delete the source file here before failing with a FileNotFoundException at the Move call later.
                    // The file would then be unrecoverable.
                    // This must obviously be avoided.
                    // We are doing this by locking the file with a stream before deleting.
                    // If source == dest, deleting the file will fail because of the open stream
                    // and the Move call below will throw an appropriate IOException.
                    using (var lockSrcStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        File.Delete(destFileName);
                    }
                }
                catch
                {
                    // We are okay with Delete failing for two reasons:
                    // 1) Depending on the error, Move might still succeed.
                    // 2) We prefer shared exceptions (like for invalid paths) to be thrown by
                    //    Move, since the messages might be better (e.g. for paramNames in ArgumentExceptions).
                    //    Furthermore, there might be functional differences between Delete and Move
                    //    regarding error handling/error severity.
                }
            }

            File.Move(sourceFileName, destFileName);
        }
#else
        internal static void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            File.Move(sourceFileName, destFileName, overwrite);
        }
#endif
    }
}
