namespace Files.Tests
{
    using Files.Tests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;
    using static Files.Tests.Mocks.StorageFileMocks;
    using static Files.Tests.Mocks.StoragePathMocks;

    [TestClass]
    public class StorageFileTests
    {
        #region AsFile Tests

        [TestMethod]
        public void AsFile_ReturnsSameInstance()
        {
            var initialFile = CreateOrdinalFileMock().Object;
            var retrievedFile = initialFile.AsFile();
            retrievedFile.ShouldBeSameAs(initialFile);
        }

        #endregion

        #region AsFolder Tests

        [TestMethod]
        public void AsFolder_ReturnsStorageFolderWithSamePathString()
        {
            var initialFile = CreateOrdinalFileMock().Object;
            var retrievedFolder = initialFile.AsFolder();
            retrievedFolder.Path.ToString().ShouldBe(initialFile.Path.ToString());
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsFullPathString()
        {
            var pathMock = CreateOrdinalPathMock();
            var fileMock = StorageFileMocks.Create(pathMock.Object, pathMock.Object.FileSystem);
            var result = fileMock.Object.ToString();
            result.ShouldBe(pathMock.Object.FullPath);
        }

        #endregion
    }
}
