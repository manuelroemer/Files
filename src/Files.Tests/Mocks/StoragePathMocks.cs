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
            Create(MockedPathString, CreateOrdinalFsMock().Object);

        public static Mock<StoragePath> CreateOrdinalIgnoreCasePathMock() =>
            Create(MockedPathString, CreateOrdinalIgnoreCaseFsMock().Object);

        public static Mock<StoragePath> CreateOrdinalUpperPathMock() =>
            Create(UpperMockedPathString, CreateOrdinalFsMock().Object);

        public static Mock<StoragePath> CreateOrdinalLowerPathMock() =>
            Create(LowerMockedPathString, CreateOrdinalFsMock().Object);

        public static Mock<StoragePath> Create(string path, FileSystem fileSystem)
        {
            var pathMock = new Mock<StoragePath>(path) { CallBase = true };
            
            pathMock
                .SetupGet(x => x.FileSystem)
                .Returns(fileSystem);
            
            return pathMock;
        }
    }
}
