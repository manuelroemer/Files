namespace Files.FileSystems.WindowsStorage.Resources
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

        internal static class WindowsStorageCompatibility
        {
            internal static string WindowsStorageElementHasNoPath() =>
                $"The current operation failed because an underlying " +
                $"{nameof(Windows)}.{nameof(Windows.Storage)}.{nameof(Windows.Storage.IStorageItem)} " +
                $"which is used by this file system implementation does not provide an actual physical " +
                $"path within the file system. " +
                $"This can happen if the file is not stored in the actual file system.";
        }

        internal static class File
        {
            internal static string ParentFolderDoesNotExist() =>
                "The file's parent folder does not exist.";

            internal static string CannotInitializeWithRootFolderPath() =>
                "The specified path points to a root folder which cannot identify a file.";

            internal static string CannotCopyToRootLocation() =>
                "Copying the file to a root location is not possible.";
            
            internal static string CannotMoveToRootLocation() =>
                "Moving the file to a root location is not possible.";

            internal static string ConflictingFolderExistsAtFileLocation() =>
                "The operation failed because a folder exists at the file's location (or at the " +
                "destination folder if this was a copy or move operation).";

            internal static string NewNameContainsInvalidChar(PathInformation pathInformation) =>
                $"The specified name contains one or more invalid characters. " +
                $"Invalid characters are:\n" +
                $"- The directory separator character '{pathInformation.DirectorySeparatorChar}'\n" +
                $"- The alternative directory separator character '{pathInformation.AltDirectorySeparatorChar}'\n" +
                $"- The volume separator character '{pathInformation.VolumeSeparatorChar}'\n" +
                $"\n" +
                $"You can use the {nameof(FileSystem)}.{nameof(FileSystem.PathInformation)} property " +
                $"of this file's {nameof(StorageFile.FileSystem)} property to determine which characters" +
                $"are allowed.";
        }

        internal static class Folder
        {
            internal static string ConflictingFileExistsAtFolderLocation() =>
                "The operation failed because a file exists at the folder's location (or at the " +
                "destination folder if this was a copy or move operation).";

            internal static string CannotMoveToRootLocation() =>
                "Moving the folder to a root location is not possible.";

            internal static string CannotCopyToSameLocation() =>
                "Copying the folder to the same location is not possible.";

            internal static string NewNameContainsInvalidChar(PathInformation pathInformation) =>
                $"The specified name contains one or more invalid characters. " +
                $"Invalid characters are:\n" +
                $"- The directory separator character '{pathInformation.DirectorySeparatorChar}'\n" +
                $"- The alternative directory separator character '{pathInformation.AltDirectorySeparatorChar}'\n" +
                $"- The volume separator character '{pathInformation.VolumeSeparatorChar}'\n" +
                $"\n" +
                $"You can use the {nameof(FileSystem)}.{nameof(FileSystem.PathInformation)} property " +
                $"of this folder's {nameof(StorageFolder.FileSystem)} property to determine which characters" +
                $"are allowed.";
        }
    }
}
