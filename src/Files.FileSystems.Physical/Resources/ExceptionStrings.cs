namespace Files.FileSystems.Physical.Resources
{
    using System;

    internal static partial class ExceptionStrings
    {

        internal static class Enum
        {

            public static string UnsupportedValue<T>(T option) where T : System.Enum =>
                $"\"{typeof(T).Name}.{option.ToString()}\" is not supported.";

        }

        internal static class String
        {

            public static string CannotBeEmpty() =>
                "The argument cannot be an empty string.";

        }

        internal static class Path
        {

            public static string InvalidFormat() =>
                $"The specified path has an invalid format. See the inner exception for details.";

            public static string TrimmingResultsInInvalidPath() =>
                "Trimming the path results in an invalid path. See the inner exception for details.";

        }

        internal static class File
        {

            public static string CannotInitializeWithRootFolderPath() =>
                "The specified path points to a root folder which cannot identify a file.";

        }

        internal static class Folder
        {

            public static string CreateFailFolderAlreadyExists() =>
                "Creating the folder failed because another folder already exists at the location.";

        }

    }

}
