using System;

namespace Files.Resources
{
    internal static partial class ExceptionStrings
    {

        public static class Path
        {

            public static string NotEmpty() =>
                "A path cannot be created from an empty string.";

        }

        public static class FileSystem
        {

            public static string SpecialFolderNotSupported(Environment.SpecialFolder value) =>
                $"The file system doesn't support creating a path or folder from the " +
                $"\"{nameof(Environment)}.{nameof(Environment.SpecialFolder)}.{value}\" enumeration value.";

        }

        internal static class File
        {

            public static string HasNoParentPath() =>
                "The parent folder of the current file could not be resolved from the path. " +
                "This is most likely an error in the underlying file system implementation, " +
                "because each file should have a parent folder.";

        }

    }

}
