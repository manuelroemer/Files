namespace Files.Tests.Mocks
{
    using Moq;
    using static Files.Tests.Mocks.FileSystemMocks;
    using static Files.Tests.Mocks.StoragePathMocks;

    public static class StorageFolderMocks
    {
        public static Mock<StorageFolder> CreateOrdinalFolderMock() =>
            Create(CreateOrdinalFsMock().Object, CreateOrdinalPathMock().Object);

        public static Mock<StorageFolder> Create(FileSystem fileSystem, StoragePath path)
        {
            return new Mock<StorageFolder>(fileSystem, path) { CallBase = true };
        }
    }
}
