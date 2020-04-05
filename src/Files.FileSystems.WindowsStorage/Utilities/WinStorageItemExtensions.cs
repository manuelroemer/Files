﻿namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System.IO;
    using Files.FileSystems.WindowsStorage.Resources;
    using Windows.Storage;

    internal static class WinStorageItemExtensions
    {
        public static string GetPathOrThrow(this IStorageItem storageItem)
        {
            return storageItem.Path ?? throw new IOException(
                ExceptionStrings.WindowsStorageCompatibility.WindowsStorageElementHasNoPath()
            );
        }
    }
}