namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System;
    using WinCreationCollisionOption = Windows.Storage.CreationCollisionOption;
    using WinNameCollisionOption = Windows.Storage.NameCollisionOption;
    using WinFileAttributes = Windows.Storage.FileAttributes;
    using IOFileAttributes = System.IO.FileAttributes;
    using Files.Shared;

    internal static class ConversionExtensions
    {
        public static WinCreationCollisionOption ToWinOptions(this CreationCollisionOption options) => options switch
        {
            CreationCollisionOption.Fail => WinCreationCollisionOption.FailIfExists,
            CreationCollisionOption.ReplaceExisting => WinCreationCollisionOption.ReplaceExisting,
            CreationCollisionOption.Ignore => WinCreationCollisionOption.OpenIfExists,
            _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
        };

        public static WinNameCollisionOption ToWinOptions(this NameCollisionOption options) => options switch
        {
            NameCollisionOption.Fail => WinNameCollisionOption.FailIfExists,
            NameCollisionOption.ReplaceExisting => WinNameCollisionOption.ReplaceExisting,
            _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
        };

        public static IOFileAttributes ToIOFileAttributes(this WinFileAttributes fileAttributes) => fileAttributes switch
        {
            // Normal is the only attribute with a different value (0 vs. 128).
            WinFileAttributes.Normal => IOFileAttributes.Normal,
            _ => (IOFileAttributes)fileAttributes,
        };

        public static WinFileAttributes ToWinFileAttributes(this IOFileAttributes fileAttributes) => fileAttributes switch
        {
            // Normal is the only attribute with a different value (0 vs. 128).
            IOFileAttributes.Normal => WinFileAttributes.Normal,
            _ => (WinFileAttributes)fileAttributes,
        };
    }
}
