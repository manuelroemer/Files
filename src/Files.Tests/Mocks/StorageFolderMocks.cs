namespace Files.Tests.Mocks
{
    using Moq;
    using static Files.Tests.Mocks.FileSystemMocks;
    using static Files.Tests.Mocks.StoragePathMocks;

    public static class StorageFolderMocks
    {
        public static Mock<StorageFolder> CreateOrdinalFolderMock() =>
            Create(CreateOrdinalPathMock().Object, CreateOrdinalFsMock().Object);

        public static Mock<StorageFolder> Create(StoragePath path, FileSystem fileSystem)
        {
            var folderMock = new Mock<StorageFolder>() { CallBase = true };

            folderMock
                .SetupGet(x => x.FileSystem)
                .Returns(fileSystem);

            folderMock
                .SetupGet(x => x.Path)
                .Returns(path);

            return folderMock;
        }
    }
}
