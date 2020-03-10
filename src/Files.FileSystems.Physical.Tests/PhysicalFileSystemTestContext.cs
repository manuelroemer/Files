namespace Files.FileSystems.Physical.Tests
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Files;
    using Files.Specification.Tests;

    public class PhysicalFileSystemTestContext : FileSystemTestContext
    {

        public static PhysicalFileSystemTestContext Instance { get; } = new PhysicalFileSystemTestContext();

        public override FileSystem FileSystem => PhysicalFileSystem.Default;

        private PhysicalFileSystemTestContext() { }

        public override async Task<StorageFolder> GetTestFolderAsync()
        {
            var folder = FileSystem
                .GetFolder(KnownFolder.TemporaryData)
                .GetFolder(Assembly.GetExecutingAssembly().GetName().Name ?? "")
                .GetFolder("TestFolder");
            await folder.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
            return folder;
        }

    }

}
