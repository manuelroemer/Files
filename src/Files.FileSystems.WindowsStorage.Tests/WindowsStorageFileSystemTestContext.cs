namespace Files.FileSystems.WindowsStorage.Tests
{
    using System.Threading.Tasks;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;

    public sealed class WindowsStorageFileSystemTestContext : FileSystemTestContext
    {
        public static WindowsStorageFileSystemTestContext Instance { get; } =
            new WindowsStorageFileSystemTestContext();

        public override FileSystem FileSystem { get; } = new WindowsStorageFileSystem();

        private WindowsStorageFileSystemTestContext()
        {
            Default.Setup(FileSystem);
        }

        public override async Task<StorageFolder> GetTestFolderAsync()
        {
            var folder = FileSystem
                .GetFolder(KnownFolder.TemporaryData)
                .GetFolder("TestFolder");
            await folder.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
            return folder;
        }
    }
}
