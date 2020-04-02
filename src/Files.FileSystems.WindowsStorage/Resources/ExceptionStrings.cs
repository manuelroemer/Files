namespace Files.FileSystems.WindowsStorage.Resources
{
    internal static class ExceptionStrings
    {
        internal static class Enum
        {
            internal static string UnsupportedValue<T>(T option) where T : System.Enum =>
                $"\"{typeof(T).Name}.{option.ToString()}\" is not supported.";
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
            internal static string CannotInitializeWithRootFolderPath() =>
                "The specified path points to a root folder which cannot identify a file.";
        }
    }
}
