namespace Files.FileSystems.InMemory
{
    using System;
    using System.Linq;
    using Files;
    using Files.FileSystems.InMemory.FsTree;
    using Files.Shared;

    /// <summary>
    ///     A <see cref="FileSystem"/> implementation specifically designed for testing which only
    ///     exists in-memory and therefore has no dependency or influence on a real file system.
    ///     See remarks for details.
    /// </summary>
    /// <remarks>
    ///     The <see cref="InMemoryFileSystem"/> only exists in-memory. Not having a dependency on
    ///     a real file system brings many advantages: You are able to run/test components which
    ///     require a <see cref="FileSystem"/> in any situation and on any machine. You don't have
    ///     to worry about cleaning up test data and you can guarantee that the tests are deterministic,
    ///     since each <see cref="InMemoryFileSystem"/> instance will always start in the same state.
    ///     Last but not least, the <see cref="InMemoryFileSystem"/> can, in comparison to a real-world
    ///     file system, be configured to some extent. Via the <see cref="InMemoryFileSystemOptions"/>,
    ///     you are, for example, able to define exactly which paths are mapped to which file system
    ///     element or which characters are used as the directory separator(s).
    ///     
    ///     One important difference between the <see cref="InMemoryFileSystem"/> and a real-world
    ///     file system implementation is that each <see cref="InMemoryFileSystem"/> instance is
    ///     isolated. Two different instances of the <c>PhysicalFileSystem</c>, for example, still
    ///     operate on the same file system of the local machine. This is not true for two different
    ///     <see cref="InMemoryFileSystem"/> instances. Each "contains" different data and might
    ///     even use different <see cref="StoragePath"/> implementations and rules.
    ///     For this reason, the <see cref="InMemoryFileSystem"/> is more strict on which parameters
    ///     it allows. The <c>PhysicalFileSystem</c> will, for example, allow a <see cref="StoragePath"/>
    ///     instance created by another <c>PhysicalFileSystem</c> instance in the
    ///     <see cref="FileSystem.GetFile(StoragePath)"/> method. The <see cref="InMemoryFileSystem"/>
    ///     will throw an exception here.
    ///     Therefore it is essential that you correctly implement your code and, ideally, don't
    ///     interchange different <see cref="FileSystem"/> instances.
    /// </remarks>
    public sealed class InMemoryFileSystem : FileSystem
    {
        internal InMemoryFileSystemOptions Options { get; }

        /// <summary>Care: This property is used for thread synchronization and is locked upon.</summary>
        internal FsDataStorage Storage { get; }

        internal char[] InvalidNewNameCharacters { get; }

        public InMemoryFileSystem()
            : this(new InMemoryFileSystemOptions()) { }

        public InMemoryFileSystem(InMemoryFileSystemOptions options)
            : base((options ?? throw new ArgumentNullException(nameof(options))).PathProvider.PathInformation)
        {
            Options = options.Clone();
            Storage = new FsDataStorage(options.StoragePathComparer);

            InvalidNewNameCharacters = new[]
                {
                    PathInformation.DirectorySeparatorChar,
                    PathInformation.AltDirectorySeparatorChar,
                    PathInformation.VolumeSeparatorChar,
                }
                .Distinct()
                .ToArray();
        }

        /// <inheritdoc/>
        public override StorageFile GetFile(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            if (!ReferenceEquals(path.FileSystem, this))
            {
                throw new ArgumentException(
                    ExceptionStrings.InMemoryFileSystem.MemberIncompatibleWithInstance(),
                    nameof(path)
                );
            }

            return new InMemoryStorageFile(this, path);
        }

        /// <inheritdoc/>
        public override StorageFolder GetFolder(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            if (!ReferenceEquals(path.FileSystem, this))
            {
                throw new ArgumentException(
                    ExceptionStrings.InMemoryFileSystem.MemberIncompatibleWithInstance(),
                    nameof(path)
                );
            }

            return new InMemoryStorageFolder(this, path);
        }

        /// <inheritdoc/>
        public override StoragePath GetPath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return Options.PathProvider.GetPath(this, path);
        }

        /// <inheritdoc/>
        public override StoragePath GetPath(KnownFolder knownFolder)
        {
            if (!EnumInfo.IsDefined(knownFolder))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(knownFolder), nameof(knownFolder));
            }
            return Options.KnownFolderProvider.GetPath(this, knownFolder);
        }
    }
}
