namespace Files.FileSystems.InMemory
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    public sealed class InMemoryFileSystemOptions
    {
        private static readonly IEqualityComparer<StoragePath> DefaultStoragePathComparer =
            DefaultStoragePathEqualityComparer.Default;
        private static readonly IInMemoryPathProvider DefaultPathProvider =
            new DefaultInMemoryPathProvider(); // TODO!
        private static readonly Encoding DefaultDefaultEncoding = Encoding.UTF8;

        private IEqualityComparer<StoragePath>? _storagePathComparer;
        private IInMemoryPathProvider? _pathProvider;
        private Encoding? _defaultEncoding;

        [AllowNull]
        public IEqualityComparer<StoragePath> StoragePathComparer
        {
            get => _storagePathComparer ?? DefaultStoragePathComparer;
            set => _storagePathComparer = value;
        }

        [AllowNull]
        public IInMemoryPathProvider PathProvider
        {
            get => _pathProvider ?? DefaultPathProvider;
            set => _pathProvider = value;
        }

        [AllowNull]
        public Encoding DefaultEncoding
        {
            get => _defaultEncoding ?? DefaultDefaultEncoding;
            set => _defaultEncoding = value;
        }
    }
}
