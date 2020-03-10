namespace Files.Specification.Tests
{
    using System.IO;
    using System.Threading.Tasks;
    using Files.Specification.Tests.Preparation;
    using Files.Specification.Tests.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class StorageFolderSpecificationTests : FileSystemTestBase
    {

        public StorageFolderSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region CreateAsync Tests

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail, true)]
        [DataRow(CreationCollisionOption.ReplaceExisting, true)]
        [DataRow(CreationCollisionOption.Ignore, true)]
        [DataRow(CreationCollisionOption.Fail, false)]
        [DataRow(CreationCollisionOption.ReplaceExisting, false)]
        [DataRow(CreationCollisionOption.Ignore, false)]
        public async Task CreateAsync_NonExistingFolder_CreatesFolder(CreationCollisionOption options, bool recursive)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await folder.CreateAsync(recursive, options).ConfigureAwait(false);
            await Assert.That.ElementExistsAsync(folder);
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndNonExistingParent_CreatesFolderAndParent(CreationCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderName).GetFolder(Default.FolderName);
            await folder.CreateAsync(recursive: true, options: options);
            await Assert.That.ElementExistsAsync(folder);
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_NotRecursiveAndNonExistingParent_ThrowsDirectoryNotFoundException(CreationCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderName).GetFolder(Default.FolderName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => folder.CreateAsync(recursive: false, options)
            );
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_FailAndExistingFolder_ThrowsIOException(bool recursive)
        {
            var folder = await TestFolder.SetupFolderAsync();
            await Assert.ThrowsExceptionAsync<IOException>(
                () => folder.CreateAsync(recursive, CreationCollisionOption.Fail)
            );
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_ReplaceExistingAndExistingFolder_ReplacesFolder(bool recursive)
        {
            var folder = await TestFolder.SetupFolderAsync();
            var nestedFile = await folder.SetupFileAsync();

            await folder.CreateAsync(recursive, CreationCollisionOption.ReplaceExisting);
            
            await Assert.That.ElementExistsAsync(folder);
            await Assert.That.ElementDoesNotExistAsync(nestedFile);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_IgnoreAndExistingFolder_DoesNothing(bool recursive)
        {
            var folder = await TestFolder.SetupFolderAsync();
            var nestedFile = await folder.SetupFileAsync();

            await folder.CreateAsync(recursive, CreationCollisionOption.Ignore);

            await Assert.That.ElementExistsAsync(folder);
            await Assert.That.ElementExistsAsync(nestedFile);
        }

        #endregion

        #region DeleteAsync Tests

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ExistingFolderWithoutChildren_DeletesFolder(DeletionOption options)
        {
            var folder = await TestFolder.SetupFolderAsync();
            await folder.DeleteAsync(options);
            await Assert.That.ElementDoesNotExistAsync(folder);
        }

        [TestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ExistingFolderWithChildren_DeletesFolderWithChildren(DeletionOption options)
        {
            var folder = await TestFolder.SetupFolderAsync();
            await folder.SetupFolderAsync();
            await folder.SetupFileAsync();

            await folder.DeleteAsync(options);
            await Assert.That.ElementDoesNotExistAsync(folder);
        }

        [TestMethod]
        public async Task DeleteAsync_FailAndNonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => folder.DeleteAsync(DeletionOption.Fail)
            );
        }
        
        [TestMethod]
        public async Task DeleteAsync_FailAndNonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName).GetFolder(Default.FolderName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => folder.DeleteAsync(DeletionOption.Fail)
            );
        }

        [TestMethod]
        public async Task DeleteAsync_IgnoreMissingAndNonExistingFolder_DoesNothing()
        {
            var folder = TestFolder.GetFile(Default.FolderName);
            await folder.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [TestMethod]
        public async Task DeleteAsync_IgnoreMissingAndNonExistingParent_DoesNothing()
        {
            var folder = TestFolder.GetFolder(Default.FolderName).GetFolder(Default.FolderName);
            await folder.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        #endregion

    }

}
