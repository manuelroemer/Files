namespace Files.FileSystems.Physical.Utilities
{
    using System;
    using System.IO;
    using Files.FileSystems.Physical.Resources;

    internal static class OptionExtensions
    {
        public static bool ToOverwriteBool(this NameCollisionOption options) => options switch
        {
            NameCollisionOption.Fail => false,
            NameCollisionOption.ReplaceExisting => true,
            _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
        };

        public static FileMode ToFileMode(this CreationCollisionOption options) => options switch
        {
            CreationCollisionOption.Fail => FileMode.CreateNew,
            CreationCollisionOption.ReplaceExisting => FileMode.Create,
            CreationCollisionOption.Ignore => FileMode.OpenOrCreate,
            _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
        };
    }
}
