namespace Files.FileSystems.InMemory.Tests
{
    using System;
    using System.Threading.Tasks;
    using Files;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;

    public sealed class InMemoryFileSystemTestContext : FileSystemTestContext
    {
        public static InMemoryFileSystemTestContext Instance { get; } = new InMemoryFileSystemTestContext();

        public override FileSystem FileSystem { get; }

        private InMemoryFileSystemTestContext()
        {
            // Since we want to test certain behaviors, e.g. that the DefaultInMemoryStoragePath validates
            // invalid characters, we require custom PathInformation.
            var testPathInformation = new PathInformation(
                invalidPathChars: new[] { '\0' },
                invalidFileNameChars: new[] { '\0' },
                '/',
                '\\',
                '.',
                '/',
                ".",
                "..",
                StringComparison.OrdinalIgnoreCase
            );

            var options = new InMemoryFileSystemOptions()
            {
                PathProvider = new DefaultInMemoryStoragePathProvider(testPathInformation),
            };

            FileSystem = new InMemoryFileSystem(options);

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
