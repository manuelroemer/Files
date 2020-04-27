namespace Files.Tests.Mocks
{
    using Moq;
    using static Files.Tests.Mocks.FileSystemMocks;
    using static Files.Tests.Mocks.StoragePathMocks;

    public static class StorageFileMocks
    {
        public static Mock<StorageFile> CreateOrdinalFileMock() =>
            Create(CreateOrdinalPathMock().Object, CreateOrdinalFsMock().Object);

        public static Mock<StorageFile> Create(StoragePath path, FileSystem fileSystem)
        {
            var fileMock = new Mock<StorageFile>() { CallBase = true };

            fileMock
                .SetupGet(x => x.FileSystem)
                .Returns(fileSystem);

            fileMock
                .SetupGet(x => x.Path)
                .Returns(path);

            return fileMock;
        }
    }
}
