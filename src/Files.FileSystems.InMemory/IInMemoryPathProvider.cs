namespace Files.FileSystems.InMemory
{
    public interface IInMemoryPathProvider
    {
        PathInformation PathInformation { get; }

        StoragePath GetPath(InMemoryFileSystem fileSystem, string path);

        StoragePath GetPath(InMemoryFileSystem fileSystem, KnownFolder knownFolder);
    }
}
