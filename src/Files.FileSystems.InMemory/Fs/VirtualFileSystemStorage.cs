namespace Files.FileSystems.InMemory.Fs
{
    using System.Collections.Generic;
    using Files.Utilities;
    using System.IO;
    using Files.FileSystems.InMemory.Resources;

    internal sealed class VirtualFileSystemStorage
    {

        private readonly Dictionary<InMemoryPath, VirtualFileSystemElement> _data;

        public VirtualFileSystemStorage()
        {
            _data = new Dictionary<InMemoryPath, VirtualFileSystemElement>(InMemoryPathEqualityComparer.Default);
        }

        public bool TryAdd(VirtualFileSystemElement element)
        {
            if (ContainsElement(element.Path))
            {
                return false;
            }

            _data.Add(element.Path, element);
            return true;
        }

        public bool TryRemove(VirtualFileSystemElement element) =>
            _data.Remove(element.Path);

        public void Move(VirtualFileSystemElement element, InMemoryPath oldPath, InMemoryPath newPath)
        {
            if (oldPath.LocationEquals(newPath))
            {
                return;
            }

            if (!ContainsElement(oldPath))
            {
                throw IOExceptionHelper.CreateNotFoundException(element, ExceptionStrings.IOExceptions.ElementNotFound(element.Path));
            }

            if (ContainsElement(newPath))
            {
                throw new IOException(ExceptionStrings.IOExceptions.ElementAlreadyExists(element.Path));
            }

            _data.Remove(element.Path);
            _data.Add(newPath, element);
        }

        public bool ContainsElement(InMemoryPath path) =>
            _data.ContainsKey(path);

        public bool ContainsFile(InMemoryPath path) =>
            _data.GetValueOrDefault(path) is VirtualFile;

        public bool ContainsFolder(InMemoryPath path) =>
            _data.GetValueOrDefault(path) is VirtualFolder;

        public VirtualFileSystemElement? TryGetElement(InMemoryPath path) =>
            _data.GetValueOrDefault(path);

        public VirtualFile? TryGetFile(InMemoryPath path) =>
            TryGetElement(path) as VirtualFile;

        public VirtualFolder? TryGetFolder(InMemoryPath path) =>
            TryGetElement(path) as VirtualFolder;

    }

}
