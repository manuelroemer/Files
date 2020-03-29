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

        internal static class Path
        {
            internal static string InvalidFormat() =>
                $"The specified path has an invalid format. See the inner exception for details.";

            internal static string TrimmingResultsInEmptyPath() =>
                $"Trimming the trailing directory separator results in an empty path string which " +
                $"is not supported by the {nameof(StoragePath)} class.";

            internal static string TrimmingResultsInInvalidPath() =>
                "Trimming the trailing directory separator results in an invalid path string. " +
                "See the inner exception for details.";
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
                $"- The (alt) directory separator character\n" +
                $"- The volume separator character\n" +
                $"- Any invalid path character\n" +
                $"- Any invalid file name character\n" +
                $"\n" +
                $"You can use the {nameof(FileSystem)}.{nameof(FileSystem.PathInformation)} property " +
                $"of this file's {nameof(FileSystem)} property to determine which characters are allowed.";
        }

        internal static class Folder
        {
            internal static string CreateFailFolderAlreadyExists() =>
                "Creating the folder failed because another folder already exists at the location.";

            internal static string ConflictingFileExistsAtFolderLocation() =>
                "The operation failed because a file exists at the folder's location.";

            internal static string CopyConflictingFolderExistsAtDestination() =>
                "Another folder already exists at the destination.";

            internal static string RenameAsyncMovesFileOutOfFolder() =>
                "The specified name is invalid, because it would move the folder out of its current folder. " +
                "Please use a normal folder name without path segments.";

            internal static string NewNameContainsInvalidChar() =>
                $"The specified name contains one or more invalid characters. " +
                $"Invalid characters are:\n" +
                $"- The (alt) directory separator character\n" +
                $"- The volume separator character\n" +
                $"- Any invalid path character\n" +
                $"\n" +
                $"You can use the {nameof(FileSystem)}.{nameof(FileSystem.PathInformation)} property " +
                $"of this folder's {nameof(FileSystem)} property to determine which characters are allowed.";
        }
    }
}
