namespace Files.Specification.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Files.Specification.Tests.Assertions;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    public abstract class StorageFolderSpecificationTests : FileSystemTestBase
    {

        public StorageFolderSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region FileSystem Tests

        [TestMethod]
        public void FileSystem_StandardFolder_IsSameInstanceAsOfParentFolder()
        {
            // This is obviously not the best test case, as the file system could be a different
            // instance in a lot of other scenarios.
            // Testing all of them is incredibly tedious with little value gained though, so
            // I trust on common sense of library implementers (including myself) here.
            // I might regret that.
            var file = TestFolder.GetFolder(Default.FolderName);
            file.FileSystem.ShouldBeSameAs(TestFolder.FileSystem);
        }

        #endregion

        #region Path Tests

        [TestMethod]
        public void Path_StandardFolder_ReturnsExpectedPath()
        {
            // This test doesn't test that the property is ALWAYS implemented correctly.
            // To do that, we'd have run this test after each method which creates a new instance (e.g.
            // after calling MoveAsync, CopyAsync, GetFolder, ...), because the value can always be set
            // wrongly there.
            // That is way beyond the scope of this specification though.
            var folder = TestFolder.GetFolder(Default.FolderName);
            var parentPath = TestFolder.Path;
            var expectedPath = parentPath.Join(Default.FolderName);
            folder.Path.ShouldBeEffectivelyEqualTo(expectedPath);
        }

        #endregion

        #region GetParent Tests

        [TestMethod]
        public void GetParent_StandardFolder_ReturnsParentFolder()
        {
            var expectedParent = TestFolder;
            var actualParent = TestFolder.GetFolder(Default.FolderName).GetParent();
            actualParent?.Path.ShouldBeEffectivelyEqualTo(expectedParent.Path);
        }

        [TestMethod]
        public void GetParent_FolderInRootFolder_ReturnsRootFolder()
        {
            var expectedParent = RootFolder;
            var actualParent = RootFolder.GetFolder(Default.FolderName).GetParent();
            actualParent?.Path.ShouldBeEffectivelyEqualTo(expectedParent.Path);
        }

        [TestMethod]
        public void GetParent_RootFolder_ReturnsNull()
        {
            RootFolder.GetParent().ShouldBeNull();
        }

        #endregion

        #region GetAttributesAsync Tests

        [TestMethod]
        public async Task GetAttributesAsync_ExistingFile_DoesNotThrow()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.GetAttributesAsync();
            // Should not throw. We cannot know which attributes are present.
        }

        [TestMethod]
        public async Task GetAttributesAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await folder.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await folder.GetAttributesAsync());
        }

        #endregion

        #region SetAttributesAsync Tests

        [TestMethod]
        public async Task SetAttributesAsync_ExistingFile_DoesNotThrow()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.SetAttributesAsync(FileAttributes.Normal);
            // Should not throw. We cannot know which attributes can be set.
        }

        [TestMethod]
        public async Task SetAttributes_InvalidAttributeCombination_DoesNotThrow()
        {
            // Of course, there is no guarantee that this is invalid in every FS implementation.
            // But in most, it should be.
            var invalidAttributes = FileAttributes.Normal | FileAttributes.Directory;
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.SetAttributesAsync(invalidAttributes);
        }

        [TestMethod]
        public async Task SetAttributesAsync_NonExistingFolderThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.SetAttributesAsync(FileAttributes.Normal));
        }

        [TestMethod]
        public async Task SetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.SetAttributesAsync(FileAttributes.Normal));
        }

        [TestMethod]
        public async Task SetAttributesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await folder.SetAttributesAsync(FileAttributes.Normal));
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_ExistingFolder_ReturnsTrue()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.ShouldExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_NonExistingFolder_ReturnsFalse()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await folder.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_NonExistingParent_ReturnsFalse()
        {
            var folder = TestFolder.GetFolder(Default.FolderName, Default.FolderName);
            await folder.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_ConflictingFileExistsAtLocation_ReturnsFalse()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            await folder.ShouldNotExistAsync();
        }

        #endregion

        #region GetPropertiesAsync Tests

        [TestMethod]
        public async Task GetPropertiesAsync_ExistingFolder_ReturnsNonNullProperties()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var props = await folder.GetPropertiesAsync();
            props.ShouldNotBeNull();
            // Without any additional info, this is the best we can do.
        }

        [DataTestMethod]
        [DataRow("folder", "ext")]
        [DataRow("folderWithoutExt", null)]
        [DataRow("folder{0}with{0}many{0}extensions{0}", "finalExt")]
        public async Task GetPropertiesAsync_ExistingFolder_ReturnsPropertiesWithExpectedValues(string name, string? extension)
        {
            name = string.Format(name, FileSystem.PathInformation.ExtensionSeparatorChar);
            var extSeparator = extension is null ? (char?)null : FileSystem.PathInformation.ExtensionSeparatorChar;
            var fullName = $"{name}{extSeparator}{extension}";

            var folder = await TestFolder.SetupFolderAsync(fullName);
            var props = await folder.GetPropertiesAsync();

            // Test the props to the best of our abilities. Specialities:
            // - We cannot really test that the name returns the REAL folder name, because we don't know if
            //   the FS uses case sensitive paths.
            // - Not testing that ModifiedOn is null without any modification. We don't know if the FS leaves it blank
            //   or sets it on creation.
            // - Testing dates is somewhat hard. We only ensure that they are in a certain time range relative to now.
            var timeSinceCreation = DateTimeOffset.UtcNow - props.CreatedOn;
            var timeSinceModification = DateTimeOffset.UtcNow - props.ModifiedOn;

            props.Name.ShouldBe(fullName);
            props.NameWithoutExtension.ShouldBe(name);
            props.Extension.ShouldBe(extension);
            timeSinceCreation.ShouldBeLessThan(TimeSpan.FromSeconds(10));
            timeSinceModification?.ShouldBeLessThan(TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public async Task GetPropertiesAsync_NonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetPropertiesAsync());
        }

        [TestMethod]
        public async Task GetPropertiesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetPropertiesAsync());
        }

        [TestMethod]
        public async Task GetPropertiesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await folder.GetPropertiesAsync());
        }

        #endregion

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
            await folder.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndNonExistingParent_CreatesFolderAndParent(CreationCollisionOption options)
        {
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await folder.CreateAsync(recursive: true, options: options);
            await folder.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_NotRecursiveAndNonExistingParent_ThrowsDirectoryNotFoundException(CreationCollisionOption options)
        {
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.CreateAsync(recursive: false, options));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_FailAndExistingFolder_ThrowsIOException(bool recursive)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.CreateAsync(recursive, CreationCollisionOption.Fail));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_ReplaceExistingAndExistingFolder_ReplacesFolder(bool recursive)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var nestedFile = await folder.SetupFileAsync(Default.FileName);

            await folder.CreateAsync(recursive, CreationCollisionOption.ReplaceExisting);

            await folder.ShouldExistAsync();
            await nestedFile.ShouldNotExistAsync();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_IgnoreAndExistingFolder_DoesNothing(bool recursive)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var nestedFile = await folder.SetupFileAsync(Default.FileName);

            await folder.CreateAsync(recursive, CreationCollisionOption.Ignore);

            await folder.ShouldExistAsync();
            await nestedFile.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail, true)]
        [DataRow(CreationCollisionOption.ReplaceExisting, true)]
        [DataRow(CreationCollisionOption.Ignore, true)]
        [DataRow(CreationCollisionOption.Fail, false)]
        [DataRow(CreationCollisionOption.ReplaceExisting, false)]
        [DataRow(CreationCollisionOption.Ignore, false)]
        public async Task CreateAsync_ConflictingFileExistsAtLocation_ThrowsIOException(CreationCollisionOption options, bool recursive)
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await folder.CreateAsync(recursive, options));
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndConflictingFileExistsAtParentLocation_ThrowsIOException(CreationCollisionOption options)
        {
            var parentFolder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            var thisFolder = parentFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<IOException>(async () => await thisFolder.CreateAsync(recursive: true, options));
        }

        #endregion

        #region DeleteAsync Tests

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ExistingFolderWithoutChildren_DeletesFolder(DeletionOption options)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.DeleteAsync(options);
            await folder.ShouldNotExistAsync();
        }

        [TestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ExistingFolderWithChildren_DeletesFolderWithChildren(DeletionOption options)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.SetupFolderAsync(Default.FolderName);
            await folder.SetupFileAsync(Default.FileName);
            await folder.DeleteAsync(options);
            await folder.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task DeleteAsync_FailAndNonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.DeleteAsync(DeletionOption.Fail));
        }

        [TestMethod]
        public async Task DeleteAsync_FailAndNonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.DeleteAsync(DeletionOption.Fail));
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
            var folder = TestFolder.GetFolderWithNonExistingParent();
            await folder.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ConflictingFileExistsAtLocation_ThrowsIOException(DeletionOption options)
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await folder.DeleteAsync(options));
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsFullPathString()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            folder.ToString().ShouldBe(folder.Path.FullPath.ToString());
        }

        #endregion

    }

}
