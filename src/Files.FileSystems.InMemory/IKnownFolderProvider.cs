namespace Files.FileSystems.InMemory
{
    public interface IKnownFolderProvider
    {
        StoragePath GetPath(InMemoryFileSystem fileSystem, KnownFolder knownFolder);
    }
}
