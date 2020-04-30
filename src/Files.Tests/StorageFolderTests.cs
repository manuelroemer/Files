namespace Files.Tests
{
    using Files.Tests.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;
    using static Files.Tests.Mocks.StorageFolderMocks;
    using static Files.Tests.Mocks.StoragePathMocks;

    [TestClass]
    public class StorageFolderTests
    {
        #region AsFile Tests

        [TestMethod]
        public void AsFile_ReturnsStorageFileWithSamePathString()
        {
            var initialFolder = CreateOrdinalFolderMock().Object;
            var retrievedFile = initialFolder.AsFile();
            retrievedFile.Path.ToString().ShouldBe(initialFolder.Path.ToString());
        }

        #endregion

        #region AsFolder Tests

        [TestMethod]
        public void AsFolder_ReturnsSameInstance()
        {
            var initialFolder = CreateOrdinalFolderMock().Object;
            var retrievedFolder = initialFolder.AsFolder();
            retrievedFolder.ShouldBeSameAs(initialFolder);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsFullPathString()
        {
            var pathMock = CreateOrdinalPathMock();
            var folderMock = StorageFolderMocks.Create(pathMock.Object.FileSystem, pathMock.Object);
            var result = folderMock.Object.ToString();
            result.ShouldBe(pathMock.Object.FullPath);
        }

        #endregion
    }
}
