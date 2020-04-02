namespace Files.FileSystems.Physical.Resources
{
    internal static class ExceptionStrings
    {
        internal static class Enum
        {
            internal static string UnsupportedValue<T>(T option) where T : System.Enum =>
                $"\"{typeof(T).Name}.{option.ToString()}\" is not supported.";
        }

        internal static class String
        {
            internal static string CannotBeEmpty() =>
                "The argument cannot be an empty string.";
        }

        internal static class File
        {
            internal static string CannotInitializeWithRootFolderPath() =>
                "The specified path points to a root folder which cannot identify a file.";

            internal static string ConflictingFolderExistsAtFileLocation() =>
                "The operation failed because a folder exists at the file's location (or at the " +
                "destination folder if this was a copy or move operation).";

            internal static string NewNameContainsInvalidChar() =>
                $"The specified name contains one or more invalid characters. " +
                $"Invalid characters are:\n" +
                $"- The directory separator character\n" +
                $"- The alternative directory separator character\n" +
                $"- The volume separator character\n" +
                $"\n" +
                $"You can use the {nameof(FileSystem)}.{nameof(FileSystem.PathInformation)} property " +
                $"of this file's {nameof(FileSystem)} property to determine which characters are allowed.";
        }

        internal static class Folder
        {
            internal static string CreateFailFolderAlreadyExists() =>
                "Creating the folder failed because another folder already exists at the location.";

            internal static string ConflictingFileExistsAtFolderLocation() =>
                "The operation failed because a file exists at the folder's location (or at the " +
                "destination folder if this was a copy or move operation).";

            internal static string CopyConflictingFolderExistsAtDestination() =>
                "Another folder already exists at the destination.";

            internal static string NewNameContainsInvalidChar() =>
                $"The specified name contains one or more invalid characters. " +
                $"Invalid characters are:\n" +
                $"- The directory separator character\n" +
                $"- The alternative directory separator character\n" +
                $"- The volume separator character\n" +
                $"\n" +
                $"You can use the {nameof(FileSystem)}.{nameof(FileSystem.PathInformation)} property " +
                $"of this file's {nameof(FileSystem)} property to determine which characters are allowed.";
        }
    }
}
