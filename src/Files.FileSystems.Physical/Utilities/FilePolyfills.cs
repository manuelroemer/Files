namespace Files.FileSystems.Physical.Utilities
{
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class FilePolyfills
    {
#if NETSTANDARD2_0
        // For the following methods, we're simply using Task.Run, because the alternatives are harder
        // to implement/maintain than one might think.
        // See for example the implementation of ReadAllBytesAsync:
        // https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.IO.FileSystem/src/System/IO/File.cs#L762
        // 
        // Using Task.Run is certainly not good etiquette, but until it actually poses any problems,
        // we can go with it.
        // This is just a polyfill after all - ideally, this library is used from .NET Core 3.0+.

        internal static Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
        {
            return Task.Run(() => File.ReadAllBytes(path), cancellationToken);
        }

        internal static Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            return Task.Run(() => File.WriteAllBytes(path, bytes), cancellationToken);
        }

        internal static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
        {
            return Task.Run(() => File.ReadAllText(path), cancellationToken);
        }

        internal static Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        {
            return Task.Run(() => File.ReadAllText(path, encoding), cancellationToken);
        }

        internal static Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken)
        {
            return Task.Run(() => File.WriteAllText(path, contents), cancellationToken);
        }

        internal static Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken)
        {
            return Task.Run(() => File.WriteAllText(path, contents, encoding), cancellationToken);
        }
#else
        internal static Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
        {
            return File.ReadAllBytesAsync(path, cancellationToken);
        }

        internal static Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            return File.WriteAllBytesAsync(path, bytes, cancellationToken);
        }

        internal static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
        {
            return File.ReadAllTextAsync(path, cancellationToken);
        }

        internal static Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        {
            return File.ReadAllTextAsync(path, encoding, cancellationToken);
        }

        internal static Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken)
        {
            return File.WriteAllTextAsync(path, contents, cancellationToken);
        }

        internal static Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken)
        {
            return File.WriteAllTextAsync(path, contents, encoding, cancellationToken);
        }
#endif

#if NETSTANDARD2_1 || NETCOREAPP2_2 || NETCOREAPP2_1 || NETCOREAPP2_0 || NETSTANDARD2_0
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
