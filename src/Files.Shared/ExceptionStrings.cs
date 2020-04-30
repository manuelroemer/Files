namespace Files.Shared
{
    using System;

    internal static class ExceptionStrings
    {
        internal static class String
        {
            internal static string CannotBeEmpty() =>
                "The specified string must not be empty.";
        }

        internal static class Enum
        {
            internal static string UndefinedValue<T>(T value) where T : struct, System.Enum =>
                $"{value} is not a valid {typeof(T).FullName} value.";

            internal static string UnsupportedValue<T>(T value) where T : struct, System.Enum =>
                $"{typeof(T).FullName}.{value} is not supported.";
        }

        internal static class Comparable
        {
            internal static string TypeIsNotSupported(Type type) =>
                $"The object cannot be compared to objects of type {type.FullName}.";
        }

        internal static class FsCompatibility
        {
            internal static string StoragePathTypeNotSupported() =>
                $"The specified {nameof(StoragePath)} has a type that is incompatible with the " +
                $"current file system implementation. " +
                $"Ensure that you are not accidently using multiple file system implementations at " +
                $"the same or that you are appropriately converting the paths between multiple " +
                $"different implementations.";
        }

        internal static class FileSystem
        {
            internal static string KnownFolderNotSupported(KnownFolder value) =>
                $"The file system doesn't support or provide a folder matching the " +
                $"\"{nameof(KnownFolder)}.{value}\" value.";
        }

        internal static class StorageFile
        {
            internal static string HasNoParentPath() =>
                "The parent folder of the current file could not be resolved from the path. " +
                "This is most likely an error in the underlying file system implementation, " +
                "because each file should have a parent folder.";

            internal static string CannotInitializeWithRootFolderPath() =>
                "The specified path points to a root folder which cannot identify a file.";

            internal static string ParentFolderDoesNotExist() =>
                "The file's parent folder does not exist.";

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
                $"You can use the {nameof(Files.FileSystem)}.{nameof(Files.FileSystem.PathInformation)} property " +
                $"of this file's {nameof(Files.StorageFile.FileSystem)} property to determine which characters" +
                $"are allowed.";

            internal static string CannotMoveToSameLocation() =>
                "Moving the file to the same location is not possible.";

            internal static string CannotCopyToRootLocation() =>
                "Copying the file to a root location is not possible.";

            internal static string CannotMoveToRootLocation() =>
                "Moving the file to a root location is not possible.";
        }

        internal static class StorageFolder
        {
            internal static string CreateFailFolderAlreadyExists() =>
                "Creating the folder failed because another folder already exists at the location.";

            internal static string ConflictingFileExistsAtFolderLocation() =>
                "The operation failed because a file exists at the folder's location (or at the " +
                "destination folder if this was a copy or move operation).";

            internal static string CopyConflictingFolderExistsAtDestination() =>
                "Another folder already exists at the destination.";

            internal static string NewNameContainsInvalidChar(PathInformation pathInformation) =>
                $"The specified name contains one or more invalid characters. " +
                $"Invalid characters are:\n" +
                $"- The directory separator character '{pathInformation.DirectorySeparatorChar}'\n" +
                $"- The alternative directory separator character '{pathInformation.AltDirectorySeparatorChar}'\n" +
                $"- The volume separator character '{pathInformation.VolumeSeparatorChar}'\n" +
                $"\n" +
                $"You can use the {nameof(Files.FileSystem)}.{nameof(Files.FileSystem.PathInformation)} property " +
                $"of this folder's {nameof(Files.StorageFolder.FileSystem)} property to determine which characters" +
                $"are allowed.";

            internal static string CannotCopyToSameLocation() =>
                "Copying the folder to the same location is not possible.";

            internal static string CannotMoveToSameLocation() =>
                "Moving the folder to the same location is not possible.";

            internal static string CannotMoveToRootLocation() =>
                "Moving the folder to a root location is not possible.";
        }

        internal static class WindowsStorageCompatibility
        {
            internal static string WindowsStorageElementHasNoPath() =>
                $"The current operation failed because an underlying Windows.Storage.IStorageItem " +
                $"which is used by this file system implementation does not provide an actual physical " +
                $"path within the file system. " +
                $"This can happen if the file is not stored in the actual file system.";
        }

        internal static class PhysicalStoragePath
        {
            internal static string InvalidFormat() =>
                $"The specified path has an invalid format.";

            internal static string TrimmingResultsInEmptyPath() =>
                $"Trimming the trailing directory separator results in an empty path string which " +
                $"is not supported by the {nameof(StoragePath)} class.";

            internal static string TrimmingResultsInInvalidPath() =>
                "Trimming the trailing directory separator results in an invalid path.";
        }
    }
}
