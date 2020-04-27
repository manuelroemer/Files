namespace Files.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;
    using static Files.Tests.Mocks.StorageFolderMocks;

    [TestClass]
    public class StorageFolderTests
    {
        #region AsFile Tests

        [TestMethod]
        public void AsFile_StorageFolder_ReturnsStorageFileWithSamePathString()
        {
            var initialFolder = CreateOrdinalFolderMock().Object;
            var retrievedFile = initialFolder.AsFile();
            retrievedFile.Path.ToString().ShouldBe(initialFolder.Path.ToString());
        }

        #endregion

        #region AsFolder Tests

        [TestMethod]
        public void AsFolder_StorageFolder_ReturnsSameInstance()
        {
            var initialFolder = CreateOrdinalFolderMock().Object;
            var retrievedFolder = initialFolder.AsFolder();
            retrievedFolder.ShouldBeSameAs(initialFolder);
        }

        #endregion
    }
}
