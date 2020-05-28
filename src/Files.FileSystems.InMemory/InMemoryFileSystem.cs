namespace Files.FileSystems.InMemory
{
    using System;
    using System.Linq;
    using Files;
    using Files.FileSystems.InMemory.Internal;
    using Files.Shared;

    public sealed class InMemoryFileSystem : FileSystem
    {
        internal InMemoryFileSystemOptions Options { get; }

        internal FsDataStorage Storage { get; }

        internal char[] InvalidNewNameCharacters { get; }

        public InMemoryFileSystem()
            : this(new InMemoryFileSystemOptions()) { }

        public InMemoryFileSystem(InMemoryFileSystemOptions options)
            : base((options ?? throw new ArgumentNullException(nameof(options))).PathProvider.PathInformation)
        {
            Options = options;
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

        public override StoragePath GetPath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return Options.PathProvider.GetPath(this, path);
        }

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
