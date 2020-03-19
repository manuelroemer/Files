﻿namespace Files.FileSystems.Physical.Resources
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

            internal static string TrimmingResultsInInvalidPath() =>
                "Trimming the path results in an invalid path. See the inner exception for details.";

        }

        internal static class File
        {

            internal static string CannotInitializeWithRootFolderPath() =>
                "The specified path points to a root folder which cannot identify a file.";

            internal static string ConflictingFolderExistsAtFileLocation() =>
                "The operation failed because a folder exists at the file's location.";

        }

        internal static class Folder
        {

            internal static string CreateFailFolderAlreadyExists() =>
                "Creating the folder failed because another folder already exists at the location.";

            internal static string ConflictingFileExistsAtFolderLocation() =>
                "The operation failed because a file exists at the folder's location.";

            internal static string CopyConflictingFolderExistsAtDestination() =>
                "Another folder already exists at the destination.";

        }

    }

}
