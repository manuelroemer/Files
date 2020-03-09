namespace Files.FileSystems.InMemory.Impl
{
    using System.Collections.Generic;
    using Files.FileSystems.InMemory.Fs;
    using Files.Utilities;

    internal sealed class FsDataStorage
    {

        private readonly object _lock = new object();
        private readonly Dictionary<InMemoryPath, FsElementData> _elements;

        public FsDataStorage()
        {
            _elements = new Dictionary<InMemoryPath, FsElementData>(InMemoryPathEqualityComparer.Default);
        }

        public void Add(FsElementData element)
        {
            lock (_lock)
            {
                _elements[element.Path] = element;
            }
        }

        public bool Remove(FsElementData element)
        {
            lock (_lock)
            {
                return _elements.Remove(element.Path);
            }
        }

        public void CreateFile(InMemoryPath path)
        {

        }

        public FsElementData? TryGetElement(InMemoryPath path)
        {
            lock (_lock)
            {
                return _elements.GetValueOrDefault(path);
            }
        }

        public FileData? TryGetFile(InMemoryPath path)
        {
            lock (_lock)
            {
                return _elements.GetValueOrDefault(path) as FileData;
            }
        }

        public FolderData? TryGetFolder(InMemoryPath path)
        {
            lock (_lock)
            {
                return _elements.GetValueOrDefault(path) as FolderData;
            }
        }

    }

}
