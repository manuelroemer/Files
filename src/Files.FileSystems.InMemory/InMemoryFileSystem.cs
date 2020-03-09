namespace Files.FileSystems.InMemory
{
    using System;
    using Files;
    using Files.FileSystems.InMemory.Fs;

    /// <summary>
    ///     A thread-safe, single-threaded, in-memory <see cref="FileSystem"/> implementation.
    /// </summary>
    public sealed class InMemoryFileSystem : FileSystem
    {

        internal VirtualFileSystemStorage Storage { get; }

        internal IInMemoryPathFactory PathFactory { get; }

        internal object Lock { get; } = new object();

        public InMemoryFileSystem(IInMemoryPathFactory pathFactory)
        {
            PathFactory = pathFactory ?? throw new ArgumentNullException(nameof(pathFactory));
            Storage = new VirtualFileSystemStorage();
        }

        public override Path GetPath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return PathFactory.Create(path);
        }

        public override Path GetPath(KnownFolder knownFolder)
        {
            throw new NotImplementedException();
        }

    }

}
