namespace Files.Tests.Mocks
{
    using System;
    using Moq;

    public static class FileSystemMocks
    {
        public static readonly PathInformation PathInfoOrdinal = new PathInformation(
            invalidPathChars: Array.Empty<char>(),
            invalidFileNameChars: Array.Empty<char>(),
            directorySeparatorChar: '/',
            altDirectorySeparatorChar: '\\',
            extensionSeparatorChar: '.',
            volumeSeparatorChar: ':',
            currentDirectorySegment: ".",
            parentDirectorySegment: "..",
            defaultStringComparison: StringComparison.Ordinal
        );

        public static readonly PathInformation PathInfoOrdinalIgnoreCase = new PathInformation(
            invalidPathChars: Array.Empty<char>(),
            invalidFileNameChars: Array.Empty<char>(),
            directorySeparatorChar: '/',
            altDirectorySeparatorChar: '\\',
            extensionSeparatorChar: '.',
            volumeSeparatorChar: ':',
            currentDirectorySegment: ".",
            parentDirectorySegment: "..",
            defaultStringComparison: StringComparison.OrdinalIgnoreCase
        );

        public static Mock<FileSystem> CreateOrdinalFsMock() =>
            Create(PathInfoOrdinal);

        public static Mock<FileSystem> CreateOrdinalIgnoreCaseFsMock() =>
            Create(PathInfoOrdinalIgnoreCase);

        public static Mock<FileSystem> Create(PathInformation pathInformation)
        {
            var fsMock = new Mock<FileSystem>(pathInformation) { CallBase = true };

            fsMock
                .Setup(x => x.GetPath(It.IsAny<string>()))
                .Returns((string path) => StoragePathMocks.Create(fsMock.Object, path).Object);

            fsMock
                .Setup(x => x.GetFile(It.IsAny<StoragePath>()))
                .Returns((StoragePath path) => StorageFileMocks.Create(fsMock.Object, path).Object);

            fsMock
                .Setup(x => x.GetFolder(It.IsAny<StoragePath>()))
                .Returns((StoragePath path) => StorageFolderMocks.Create(fsMock.Object, path).Object);

            return fsMock;
        }
    }
}
