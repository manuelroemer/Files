namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///     Allows configuring specific parts and behaviors of an <see cref="InMemoryFileSystem"/> instance.
    /// </summary>
    public sealed class InMemoryFileSystemOptions
    {
        private IEqualityComparer<StoragePath> _storagePathComparer = DefaultStoragePathEqualityComparer.Default;
        private IInMemoryStoragePathProvider _pathProvider = DefaultInMemoryStoragePathProvider.DefaultOrdinal;
        private IKnownFolderProvider _knownFolderProvider = DefaultKnownFolderProvider.Default;
        private Encoding _defaultEncoding = Encoding.UTF8;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InMemoryFileSystemOptions"/> with a
        ///     default set of values.
        /// </summary>
        public InMemoryFileSystemOptions() { }

        /// <summary>
        ///     Gets or sets an equality comparer which is used by the <see cref="InMemoryFileSystem"/>
        ///     to determine whether two <see cref="StoragePath"/> instances locate the same file or folder.
        ///     
        ///     The default value is a <see cref="DefaultStoragePathEqualityComparer"/> instance.
        /// </summary>
        public IEqualityComparer<StoragePath> StoragePathComparer
        {
            get => _storagePathComparer;
            set => _storagePathComparer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Gets or sets the <see cref="IInMemoryStoragePathProvider"/> which is used by the
        ///     <see cref="InMemoryFileSystem"/> to create <see cref="StoragePath"/> instances.
        ///     
        ///     The default value is a <see cref="DefaultInMemoryStoragePathProvider"/> instance.
        /// </summary>
        public IInMemoryStoragePathProvider PathProvider
        {
            get => _pathProvider;
            set => _pathProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Gets or sets the <see cref="IKnownFolderProvider"/> which is used by the
        ///     <see cref="InMemoryFileSystem"/> to create <see cref="StoragePath"/> instances
        ///     which locate <see cref="KnownFolder"/> values.
        ///     
        ///     The default value is a <see cref="DefaultKnownFolderProvider"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public IKnownFolderProvider KnownFolderProvider
        {
            get => _knownFolderProvider;
            set => _knownFolderProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Gets or sets the default encoding which is used by the <see cref="InMemoryFileSystem"/>
        ///     when reading/writing from/to files.
        ///     
        ///     The default value is <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public Encoding DefaultEncoding
        {
            get => _defaultEncoding;
            set => _defaultEncoding = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        ///     Clones the options.
        ///     Introduced in order to prevent retroactive modification once the options have been
        ///     used to initialize an <see cref="InMemoryFileSystem"/> instance.
        /// </summary>
        internal InMemoryFileSystemOptions Clone() =>
            (InMemoryFileSystemOptions)MemberwiseClone();
    }
}
