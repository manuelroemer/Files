namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System;
    using Files.FileSystems.WindowsStorage.Resources;
    using WinCreationCollisionOption = Windows.Storage.CreationCollisionOption;

    internal static class OptionExtensions
    {
        public static WinCreationCollisionOption ToWinOptions(this CreationCollisionOption options) => options switch
        {
            CreationCollisionOption.Fail => WinCreationCollisionOption.FailIfExists,
            CreationCollisionOption.ReplaceExisting => WinCreationCollisionOption.ReplaceExisting,
            CreationCollisionOption.Ignore => WinCreationCollisionOption.OpenIfExists,
            _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
        };
    }
}
