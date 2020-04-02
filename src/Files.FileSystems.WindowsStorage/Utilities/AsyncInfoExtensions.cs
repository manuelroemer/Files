namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System.Threading;
    using Windows.Foundation;

    internal static class AsyncInfoExtensions
    {
        internal static T Cancel<T>(this T asyncInfo, CancellationToken cancellationToken)
            where T : IAsyncInfo
        {
            cancellationToken.Register(() => asyncInfo.Cancel());
            return asyncInfo;
        }
    }
}
