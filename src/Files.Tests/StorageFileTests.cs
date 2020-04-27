namespace Files.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;
    using static Files.Tests.Mocks.StorageFileMocks;

    [TestClass]
    public class StorageFileTests
    {
        #region AsFile Tests

        [TestMethod]
        public void AsFile_StorageFile_ReturnsSameInstance()
        {
            var initialFile = CreateOrdinalFileMock().Object;
            var retrievedFile = initialFile.AsFile();
            retrievedFile.ShouldBeSameAs(initialFile);
        }

        #endregion

        #region AsFolder Tests

        [TestMethod]
        public void AsFolder_StorageFile_ReturnsStorageFolderWithSamePathString()
        {
            var initialFile = CreateOrdinalFileMock().Object;
            var retrievedFolder = initialFile.AsFolder();
            retrievedFolder.Path.ToString().ShouldBe(initialFile.Path.ToString());
        }

        #endregion
    }
}
