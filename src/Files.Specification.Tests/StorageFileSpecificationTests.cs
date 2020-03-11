namespace Files.Specification.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Files.Specification.Tests.Assertions;
    using Files.Specification.Tests.Setup;
    using Files.Specification.Tests.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public abstract class StorageFileSpecificationTests : FileSystemTestBase
    {

        public StorageFileSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region FileSystem Tests

        [TestMethod]
        public void FileSystem_Returns_Current_FileSystem()
        {
            // This test doesn't test that the property is ALWAYS implemented correctly.
            // To do that, we'd have run this test after each method which creates a new instance (e.g.
            // after calling MoveAsync, CopyAsync, GetFile, ...), because the value can always be set
            // wrongly there.
            // That is way beyond the scope of this specification though.
            var file = TestFolder.GetFile(Default.FileName);
            file.FileSystem.ShouldBeSameAs(TestFolder.FileSystem);
        }

        #endregion

        #region Path Tests

        [TestMethod]
        public void Path_Returns_Current_Path()
        {
            // This test doesn't test that the property is ALWAYS implemented correctly.
            // To do that, we'd have run this test after each method which creates a new instance (e.g.
            // after calling MoveAsync, CopyAsync, GetFile, ...), because the value can always be set
            // wrongly there.
            // That is way beyond the scope of this specification though.
            var parentPath = TestFolder.Path;
            var expectedPath = (parentPath / Default.FileName).TrimEndingDirectorySeparator();
            var file = TestFolder.GetFile(Default.FileName);
            var actualPath = file.Path.TrimEndingDirectorySeparator();
            actualPath.ShouldBe(expectedPath);
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
        public async Task CreateAsync_NonExistingFile_CreatesFile(CreationCollisionOption options, bool recursive)
        {
            var file = TestFolder.GetFile(Default.FileName);
            await file.CreateAsync(recursive, options).ConfigureAwait(false);
            await file.ShouldExistAsync();
        }
        
        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndNonExistingParent_CreatesFileAndParent(CreationCollisionOption options)
        {
            var file = TestFolder.GetFileWithNonExistingParent();
            await file.CreateAsync(recursive: true, options: options);
            await file.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_NotRecursiveAndNonExistingParent_ThrowsDirectoryNotFoundException(CreationCollisionOption options)
        {
            var file = TestFolder.GetFileWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.CreateAsync(recursive: false, options));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_FailAndExistingFile_ThrowsIOException(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync();
            await Should.ThrowAsync<IOException>(async () => await file.CreateAsync(recursive, CreationCollisionOption.Fail));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_ReplaceExistingAndExistingFile_ReplacesFile(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent);
            
            await file.CreateAsync(recursive, CreationCollisionOption.ReplaceExisting);

            await file.ShouldExistAsync();
            await file.ShouldHaveEmptyContentAsync();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_IgnoreAndExistingFile_DoesNothing(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent);
            
            await file.CreateAsync(recursive, CreationCollisionOption.Ignore);

            await file.ShouldExistAsync();
            await file.ShouldHaveContentAsync(Default.TextContent);
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail, true)]
        [DataRow(CreationCollisionOption.ReplaceExisting, true)]
        [DataRow(CreationCollisionOption.Ignore, true)]
        [DataRow(CreationCollisionOption.Fail, false)]
        [DataRow(CreationCollisionOption.ReplaceExisting, false)]
        [DataRow(CreationCollisionOption.Ignore, false)]
        public async Task CreateAsync_ConflictingFolderExistsAtLocation_ThrowsIOException(CreationCollisionOption options, bool recursive)
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await file.CreateAsync(recursive, options));
        }

        #endregion

        #region DeleteAsync Tests

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ExistingFile_DeletesFile(DeletionOption options)
        {
            var file = await TestFolder.SetupFileAsync();
            await file.DeleteAsync(options);
            await file.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task DeleteAsync_FailAndNonExistingFile_ThrowsFileNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.DeleteAsync(DeletionOption.Fail));
        }

        [DataTestMethod]
        public async Task DeleteAsync_FailAndNonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var file = TestFolder.GetFileWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.DeleteAsync(DeletionOption.Fail));
        }

        [TestMethod]
        public async Task DeleteAsync_IgnoreMissingAndNonExistingFile_DoesNothing()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await file.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [TestMethod]
        public async Task DeleteAsync_IgnoreMissingAndNonExistingParent_DoesNothing()
        {
            var file = TestFolder.GetFileWithNonExistingParent();
            await file.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ConflictingFolderExistsAtLocation_ThrowsIOException(DeletionOption options)
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await file.DeleteAsync(options));
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndConflictingFileExistsAtParentLocation_ThrowsIOException(CreationCollisionOption options)
        {
            var parentFolder = await TestFolder.SetupFileAndGetFolderAtSameLocation();
            var thisFile = parentFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<IOException>(async () => await thisFile.CreateAsync(recursive: true, options));
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_ExistingFile_ReturnsTrue()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.ShouldExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_NonExistingFile_ReturnsFalse()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await file.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_NonExistingParent_ReturnsFalse()
        {
            var file = TestFolder.GetFileWithNonExistingParent();
            await file.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_ConflictingFolderExistsAtLocation_ReturnsFalse()
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation();
            await file.ShouldNotExistAsync();
        }

        #endregion

        #region GetAttributesAsync Tests

        [TestMethod]
        public async Task GetAttributesAsync_ExistingFile_DoesNotThrow()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.GetAttributesAsync();
            // Should not throw. We cannot know which attributes are present.
        }

        [TestMethod]
        public async Task GetAttributesAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var file = TestFolder.GetFileWithNonExistingParent();
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation();
            await Should.ThrowAsync<IOException>(async () => await file.GetAttributesAsync());
        }

        #endregion

        #region GetParent Tests

        [TestMethod]
        public void GetParent_Returns_Parent_Element()
        {
            var initialParent = TestFolder.GetFolder(Default.FolderName);
            var file = initialParent.GetFile(Default.FileName);
            var retrievedParent = file.GetParent();
            initialParent.Path.ShouldBeEffectivelyEqualTo(retrievedParent.Path);
        }

        #endregion

        #region GetPropertiesAsync Tests

        [TestMethod]
        public async Task GetPropertiesAsync_Returns_Properties()
        {
            var file = await TestFolder.SetupFileAsync();
            var props = await file.GetPropertiesAsync();
            props.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task GetPropertiesAsync_Returns_Expected_Properties()
        {
            // Create a file with the real name "file.ext", but access it via the path "file.ext/foo/.."
            // to make things difficult.
            // The real point of getting the properties is to access the real values of the underlying FS,
            // e.g. the REAL name. By accessing the file via the relative path above, we replicate
            // the potential error that a user is unable to retrieve any file info from the path itself
            // and ensure that the underlying FS implementation doesn't use the path for the properties itself.
            var fileName = "file" + FileSystem.PathInformation.ExtensionSeparatorChar + "ext";
            await TestFolder.SetupFileAsync(basePath => basePath / fileName);
            var file = FileSystem.GetFile(TestFolder.Path / fileName / "foo" / FileSystem.PathInformation.ParentDirectorySegment);
            await file.WriteBytesAsync(Default.ByteContent);
            var props = await file.GetPropertiesAsync();

            // Test the props to the best of our abilities. Specialities:
            // - Not testing that ModifiedOn is null without any modification. We don't know if the FS leaves it blank
            //   or sets it on creation.
            // - Testing dates is somewhat hard. We only ensure that they are in a certain time range relative to now.
            var timeSinceCreation = (DateTimeOffset.UtcNow - props.CreatedOn);
            var timeSinceModification = (DateTimeOffset.UtcNow - props.ModifiedOn);
            
            props.Name.ShouldBe(fileName);
            props.NameWithoutExtension.ShouldBe("file");
            props.Extension.ShouldBe("ext");
            props.Size.ShouldBe((ulong)Default.ByteContent.Length);
            timeSinceCreation.ShouldBeLessThan(TimeSpan.FromSeconds(10));
            timeSinceModification?.ShouldBeLessThan(TimeSpan.FromSeconds(10));
        }

        #endregion

        #region CopyAsync Tests

        [TestMethod]
        public async Task CopyAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync();
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.CopyAsync(destinationPath: null!));
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Copies_File(NameCollisionOption options)
        {
            var (_, dst, srcFile) = await TestFolder.SetupSrcDstFileAsync();
            await srcFile.WriteTextAsync(Default.TextContent);

            var dstFilePath = dst.Path / Default.FileName;
            var dstFile = await srcFile.CopyAsync(dstFilePath, options: options);

            // Asserting that the parent changed is hard without path normalization.
            // This is the best we can easily check.
            srcFile.GetParent()?.Path.ShouldNotBeEffectivelyEqualTo(dstFile.GetParent()?.Path);
            await srcFile.ShouldExistAsync();
            await dstFile.ShouldExistAsync();
            await srcFile.ShouldHaveContentAsync(Default.TextContent);
            await dstFile.ShouldHaveContentAsync(Default.TextContent);
        }

        [TestMethod]
        public async Task CopyAsync_Fail_Throws_IOException_If_File_Already_Exists()
        {
            var (src, dst) = await TestFolder.SetupTwoConflictingFilesAsync();
            await Should.ThrowAsync<IOException>(async () => await src.CopyAsync(dst.Path, NameCollisionOption.Fail));
        }
        
        [TestMethod]
        public async Task CopyAsync_ReplaceExisting_Replaces_Existing_File()
        {
            var (src, dst) = await TestFolder.SetupTwoConflictingFilesAsync();
            await src.WriteTextAsync(Default.TextContent);

            await src.CopyAsync(dst.Path, NameCollisionOption.ReplaceExisting);
            await dst.ShouldHaveContentAsync(Default.TextContent);
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Throws_IOException_If_Source_Equals_Destination(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync();
            await Should.ThrowAsync<IOException>(async () => await srcFile.CopyAsync(srcFile.Path, options));
        }
        
        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Throws_DirectoryNotFoundException_If_Source_Parent_Directory_Does_Not_Exist(NameCollisionOption options)
        {
            var srcFile = TestFolder.GetFolder(Default.SrcFolderName).GetFile(Default.SrcFileName);
            var dstDir = await TestFolder.SetupFolderAsync(basePath => basePath / Default.DstFolderName);
            var dstPath = dstDir.Path / Default.DstFileName;
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await srcFile.CopyAsync(dstPath, options));
        }
        
        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Throws_DirectoryNotFoundException_If_Destination_Directory_Does_Not_Exist(NameCollisionOption options)
        {
            var src = await TestFolder.SetupFileAsync(basePath => basePath / Default.SrcFolderName / Default.FileName);
            var dstDir = TestFolder.GetFolder(Default.DstFolderName);
            var dstPath = dstDir.Path / Default.DstFileName;
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await src.CopyAsync(dstPath, options));
        }
        
        #endregion
        
        #region ReadTextAsync / WriteTextAsync Tests

        [TestMethod]
        public async Task WriteTextAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync();
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.WriteTextAsync(null!));
        }

        [TestMethod]
        public async Task Can_Read_Write_Text_With_Default_Encoding()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent);
            await file.ShouldHaveContentAsync(Default.TextContent);
        }

        [TestMethod]
        public async Task Can_Read_Write_Text_With_Specified_Encoding()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent, Encoding.ASCII);
            await file.ShouldHaveContentAsync(Default.TextContent, Encoding.ASCII);
        }

        [TestMethod]
        public async Task WriteTextAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.WriteTextAsync(Default.TextContent));
        }
        
        [TestMethod]
        public async Task ReadTextAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.ReadTextAsync());
        }
        
        [TestMethod]
        public async Task WriteTextAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.WriteTextAsync(Default.TextContent));
        }
        
        [TestMethod]
        public async Task ReadTextAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.ReadTextAsync());
        }

        #endregion

        #region ReadBytesAsync / WriteBytesAsync Tests

        [TestMethod]
        public async Task WriteBytesAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync();
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.WriteBytesAsync(null!));
        }

        [TestMethod]
        public async Task Can_Read_Write_Bytes()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteBytesAsync(Default.ByteContent);
            await file.ShouldHaveContentAsync(Default.ByteContent);
        }

        [TestMethod]
        public async Task WriteBytesAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.WriteBytesAsync(Default.ByteContent));
        }

        [TestMethod]
        public async Task ReadBytesAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.ReadBytesAsync());
        }

        [TestMethod]
        public async Task WriteBytesAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.WriteBytesAsync(Default.ByteContent));
        }

        [TestMethod]
        public async Task ReadBytesAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.ReadBytesAsync());
        }

        #endregion

    }

}
