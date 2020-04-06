using System;

namespace Files.Resources
{
    internal static class ExceptionStrings
    {
        internal static class String
        {
            internal static string CannotBeEmpty() =>
                "The argument cannot be an empty string.";
        }

        internal static class Comparable
        {
            internal static string TypeIsNotSupported(Type type) =>
                $"The object cannot be compared to objects of type {type.FullName}.";
        }

        public static class FileSystem
        {
            public static string KnownFolderNotSupported(KnownFolder value) =>
                $"The file system doesn't support creating a path or folder from the " +
                $"\"{nameof(KnownFolder)}.{value}\" enumeration value.";
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
