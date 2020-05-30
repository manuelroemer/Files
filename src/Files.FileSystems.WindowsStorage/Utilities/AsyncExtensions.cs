namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Windows.Foundation;

    internal static class AsyncExtensions
    {
        internal static ConfiguredTaskAwaitable AsAwaitable(
            this IAsyncAction asyncAction,
            CancellationToken cancellationToken
        )
        {
            return asyncAction.AsTask(cancellationToken).ConfigureAwait(false);
        }

        internal static ConfiguredTaskAwaitable<T> AsAwaitable<T>(
            this IAsyncOperation<T> asyncOperation,
            CancellationToken cancellationToken
        )
        {
            return asyncOperation.AsTask(cancellationToken).ConfigureAwait(false);
        }
    }
}
