namespace Files.FileSystems.InMemory.Tests
{
    using System.Threading.Tasks;
    using Files;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;

    public sealed class InMemoryFileSystemTestContext : FileSystemTestContext
    {
        public static InMemoryFileSystemTestContext Instance { get; } = new InMemoryFileSystemTestContext();

        public override FileSystem FileSystem => new InMemoryFileSystem();

        private InMemoryFileSystemTestContext()
        {
            Default.Setup(FileSystem);
        }

        public override async Task<StorageFolder> GetTestFolderAsync()
        {
            var folder = FileSystem.GetFolder("TestFolder");
            await folder.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
            return folder;
        }
    }
}
