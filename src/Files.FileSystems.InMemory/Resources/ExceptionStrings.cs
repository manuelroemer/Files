namespace Files.FileSystems.InMemory.Resources
{

    internal static class ExceptionStrings
    {

        public static class ReadWriteTracker
        {

            public static string ElementInUse() =>
                "The element cannot be accessed, because it is currently in use.";

        }

        public static class InMemoryFileSystemElement
        {

            public static string NameIsPath(string name) =>
                $"The specified name \"{name}\" is not a single name, but a path.";

        }

        public static class IOExceptions
        {

            public static string ElementAlreadyExists(Path path) =>
                $"An element already exists at \"{path}\".";

            public static string FileBlocksCreation(Path path) =>
                $"Cannot create a folder at \"{path}\", because a file already exists at that location.";
            
            public static string FolderBlocksCreation(Path path) =>
                $"Cannot create a file at \"{path}\", because a folder already exists at that location.";

            public static string ElementNotFound(Path path) =>
                $"No element exists at \"{path}\".";

            public static string DestinationParentNotFound(Path path) =>
                $"The destination's parent folder at \"{path}\" does not exist.";

            public static string MismatchExpectedFileButGotFolder(Path path) =>
                $"Expected the element at \"{path}\" to be a file, but found a folder.";

            public static string MismatchExpectedFolderButGotFile(Path path) =>
                $"Expected the element at \"{path}\" to be a folder, but found a file.";

            public static string CannotMoveRootDirectory(Path path) =>
                $"The path \"{path}\" points to a root directory. Root directories cannot be moved.";

            public static string CannotMoveParentIntoChild(Path source, Path destination) =>
                $"The element at \"{source}\" cannot be moved into \"{destination}\", because " +
                $"the element at the destination path is a child of the element at the source path.";

        }

    }

}
