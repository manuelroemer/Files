namespace Files.FileSystems.Physical.Utilities
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using IOPath = System.IO.Path;
    using System.IO;

    internal static class SafeFileTransferOperation
    {

        public static void TransferAsync(
            Action<string> deleteElement,
            Action<string, string> moveElement,
            Func<string, bool> elementExists,
            string transferSource,
            string transferDestination,
            bool overwrite,
            CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo? transferException = null;
            string? tempPath = null;

            cancellationToken.ThrowIfCancellationRequested();

            if (overwrite && elementExists(transferDestination))
            {
                tempPath = IOPath.Join(transferDestination, Guid.NewGuid().ToString(), ".tmp");
                moveElement(transferDestination, tempPath);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                moveElement(transferSource, transferDestination);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031
            {
                transferException = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                // If transfering the element failed, immediately restore the initial state.
                if (transferException != null)
                {
                    // Some data may have partially been transfered, so ensure that it gets deleted first.
                    try
                    {
                        deleteElement(transferDestination);
                    }
                    catch (IOException)
                    {
                    }

                    // Restore the element to the original path.
                    if (tempPath != null)
                    {
                        moveElement(tempPath, transferDestination);
                    }

                    transferException.Throw();
                }

                if (tempPath != null)
                {
                    deleteElement(tempPath);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

    }

}
