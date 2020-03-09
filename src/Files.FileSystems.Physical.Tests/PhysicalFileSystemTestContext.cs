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

        public override async Task<Folder> GetTestFolderAsync()
        {
            var tmpPath = System.IO.Path.GetTempPath();
            var testFolderPath = System.IO.Path.Join(
                tmpPath,
                Assembly.GetExecutingAssembly().GetName().Name,
                "TestFolder"
            );
            var folder = FileSystem.GetFolder(testFolderPath);
            await folder.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
            return folder;
        }

    }

}
