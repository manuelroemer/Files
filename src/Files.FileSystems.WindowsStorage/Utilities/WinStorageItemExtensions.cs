namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System.IO;
    using Files.Shared;
    using Windows.Storage;

    internal static class WinStorageItemExtensions
    {
        internal static string GetPathOrThrow(this IStorageItem storageItem)
        {
            return storageItem.Path ?? throw new IOException(
                ExceptionStrings.WindowsStorageCompatibility.WindowsStorageElementHasNoPath()
            );
        }
    }
}
