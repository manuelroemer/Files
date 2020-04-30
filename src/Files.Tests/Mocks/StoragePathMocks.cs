namespace Files.Tests.Mocks
{
    using Moq;
    using static Files.Tests.Mocks.FileSystemMocks;

    public static class StoragePathMocks
    {
        public const string MockedPathString = "MockPathString";
        public const string UpperMockedPathString = "MOCKPATHSTRING";
        public const string LowerMockedPathString = "mockpathstring";

        public static Mock<StoragePath> CreateOrdinalPathMock() =>
            Create(CreateOrdinalFsMock().Object, MockedPathString);

        public static Mock<StoragePath> CreateOrdinalIgnoreCasePathMock() =>
            Create(CreateOrdinalIgnoreCaseFsMock().Object, MockedPathString);

        public static Mock<StoragePath> CreateOrdinalUpperPathMock() =>
            Create(CreateOrdinalFsMock().Object, UpperMockedPathString);

        public static Mock<StoragePath> CreateOrdinalLowerPathMock() =>
            Create(CreateOrdinalFsMock().Object, LowerMockedPathString);

        public static Mock<StoragePath> Create(FileSystem fileSystem, string path)
        {
            var pathMock = new Mock<StoragePath>(fileSystem, path) { CallBase = true };
            
            pathMock
                .SetupGet(x => x.FullPath)
                .Returns(() => pathMock.Object);

            return pathMock;
        }
    }
}
