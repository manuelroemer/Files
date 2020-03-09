namespace Files.FileSystems.InMemory.Fs
{
    using System.IO;
    using Files.FileSystems.InMemory.Resources;

    internal sealed class VirtualFile : VirtualFileSystemElement
    {

        private VirtualFile(VirtualFileSystemStorage storage, InMemoryPath path)
            : base(storage, path) { }

        public static VirtualFile Create(VirtualFileSystemStorage storage, InMemoryPath path, bool openExisting)
        {
            if (storage.ContainsFolder(path))
            {
                throw new IOException(ExceptionStrings.IOExceptions.FolderBlocksCreation(path));
            }

            if (storage.ContainsFile(path) && !openExisting)
            {
                throw new IOException(ExceptionStrings.IOExceptions.ElementAlreadyExists(path));
            }

            // The ctor creates the whole relations in the storage.
            return storage.TryGetFile(path) ?? new VirtualFile(storage, path);
        }

        public static VirtualFile Get(VirtualFileSystemStorage storage, InMemoryPath path)
        {
            return storage.TryGetElement(path) switch
            {
                VirtualFile folder => folder,
                VirtualFolder _ => throw new IOException(ExceptionStrings.IOExceptions.MismatchExpectedFolderButGotFile(path)),
                _ => throw new FileNotFoundException(ExceptionStrings.IOExceptions.ElementNotFound(path))
            };
        }

        public static bool Exists(VirtualFileSystemStorage storage, InMemoryPath path)
        {
            return storage.TryGetFile(path) is object;
        }

        protected override IOException CreateNotFoundException()
        {
            return new FileNotFoundException(ExceptionStrings.IOExceptions.ElementNotFound(Path));
        }

    }

}
