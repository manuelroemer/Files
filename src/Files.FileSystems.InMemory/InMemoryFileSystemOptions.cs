namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class InMemoryFileSystemOptions
    {
        private IEqualityComparer<StoragePath> _storagePathComparer = DefaultStoragePathEqualityComparer.Default;
        private IInMemoryPathProvider _pathProvider = DefaultInMemoryPathProvider.DefaultOrdinal;
        private IKnownFolderProvider _knownFolderProvider = DefaultKnownFolderProvider.Default;
        private Encoding _defaultEncoding = Encoding.UTF8;

        public IEqualityComparer<StoragePath> StoragePathComparer
        {
            get => _storagePathComparer;
            set => _storagePathComparer = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IInMemoryPathProvider PathProvider
        {
            get => _pathProvider;
            set => _pathProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IKnownFolderProvider KnownFolderProvider
        {
            get => _knownFolderProvider;
            set => _knownFolderProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Encoding DefaultEncoding
        {
            get => _defaultEncoding;
            set => _defaultEncoding = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
