namespace Files.Tests.Mocks
{
    using Moq;
    using static Files.Tests.Mocks.FileSystemMocks;
    using static Files.Tests.Mocks.StoragePathMocks;

    public static class StorageFileMocks
    {
        public static Mock<StorageFile> CreateOrdinalFileMock() =>
            Create(CreateOrdinalFsMock().Object, CreateOrdinalPathMock().Object);

        public static Mock<StorageFile> Create(FileSystem fileSystem, StoragePath path)
        {
            return new Mock<StorageFile>(fileSystem, path) { CallBase = true };
        }
    }
}
