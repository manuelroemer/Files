namespace Files.FileSystems.Physical.Utilities
{
    using System;
    using System.IO;
    using Files.Shared;

    internal static class ConversionExtensions
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
            CreationCollisionOption.UseExisting => FileMode.OpenOrCreate,
            _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options)),
        };
    }
}
