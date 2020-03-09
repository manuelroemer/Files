namespace Files.FileSystems.InMemory
{
    using System;
    using Files.FileSystems.InMemory.Impl;
    using Files;

    internal sealed class InMemoryFile : File
    {

        private readonly InMemoryFileSystem _inMemoryFileSystem;
        private readonly InMemoryPath _path;
        private readonly FsDataStorage _storage;

        public override FileSystem FileSystem => _inMemoryFileSystem;

        public override Path Path => _path;

        public override string Name => _storage.TryGetFile(_path)?.Name ?? _path.Name;

        public override string NameWithoutExtension => 
            _storage.TryGetFile(_path)?.NameWithoutExtension ?? _path.FileNameWithoutExtension ?? _path.Name;

        public override string? Extension => _storage.TryGetFile(_path)?.Extension ?? _path.Extension;

        public override DateTimeOffset? CreationTime => _storage.TryGetFile(_path)?.CreationTime;

        public InMemoryFile(InMemoryFileSystem fileSystem, InMemoryPath path, FileData fileData)
        {

        }

    }

}
