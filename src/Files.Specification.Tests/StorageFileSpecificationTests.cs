namespace Files.Specification.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Files.Specification.Tests.Preparation;
    using Files.Specification.Tests.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StoragePath = StoragePath;

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
            Assert.AreSame(TestFolder.FileSystem, file.FileSystem);
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
            Assert.AreEqual(expectedPath, actualPath);
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
            await Assert.That.ElementExistsAsync(file);
        }
        
        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndNonExistingParent_CreatesFileAndParent(CreationCollisionOption options)
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await file.CreateAsync(recursive: true, options: options);
            await Assert.That.ElementExistsAsync(file);
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_NotRecursiveAndNonExistingParent_ThrowsDirectoryNotFoundException(CreationCollisionOption options)
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => file.CreateAsync(recursive: false, options)
            );
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_FailAndExistingFile_ThrowsIOException(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync();
            await Assert.ThrowsExceptionAsync<IOException>(
                () => file.CreateAsync(recursive, CreationCollisionOption.Fail)
            );
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_ReplaceExistingAndExistingFile_ReplacesFile(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent);
            
            await file.CreateAsync(recursive, CreationCollisionOption.ReplaceExisting);
            
            await Assert.That.ElementExistsAsync(file);
            await Assert.That.FileHasContentAsync(file, "");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_IgnoreAndExistingFile_DoesNothing(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent);
            
            await file.CreateAsync(recursive, CreationCollisionOption.Ignore);
            
            await Assert.That.ElementExistsAsync(file);
            await Assert.That.FileHasContentAsync(file, Default.TextContent);
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
            await Assert.That.ElementDoesNotExistAsync(file);
        }

        [TestMethod]
        public async Task DeleteAsync_FailAndNonExistingFile_ThrowsFileNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                () => file.DeleteAsync(DeletionOption.Fail)
            );
        }

        [DataTestMethod]
        public async Task DeleteAsync_FailAndNonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => file.DeleteAsync(DeletionOption.Fail)
            );
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
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await file.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        #endregion

        #region GetParent Tests

        [TestMethod]
        public void GetParent_Returns_Parent_Element()
        {
            var initialParent = TestFolder.GetFolder(Default.FolderName);
            var file = initialParent.GetFile(Default.FileName);
            var retrievedParent = file.GetParent();
            Assert.That.PathsAreEffectivelyEqual(initialParent.Path, retrievedParent.Path);
        }

        #endregion

        #region GetPropertiesAsync Tests

        [TestMethod]
        public async Task GetPropertiesAsync_Returns_Properties()
        {
            var file = await TestFolder.SetupFileAsync();
            var props = await file.GetPropertiesAsync();
            Assert.IsNotNull(props);
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
            Assert.AreEqual(fileName, props.Name, $"Name ({props.Name}) does not equal \"{fileName}\".");
            Assert.AreEqual("file", props.NameWithoutExtension, $"NameWithoutExtension ({props.NameWithoutExtension}) does not equal \"file\".");
            Assert.AreEqual("ext", props.Extension, $"Extension ({props.Extension}) does not equal \"ext\".");
            Assert.IsTrue((DateTimeOffset.UtcNow - props.CreatedOn).Ticks < TimeSpan.FromSeconds(10).Ticks, $"CreatedOn ({props.CreatedOn}) isn't set to a point in time within in the last 10 seconds.");
            Assert.IsTrue((DateTimeOffset.UtcNow - props.ModifiedOn)?.Ticks < TimeSpan.FromSeconds(10).Ticks, $"ModifiedOn ({props.ModifiedOn}) isn't set to a point in time within in the last 10 seconds.");
            Assert.AreEqual((ulong)Default.ByteContent.Length, props.Size, $"Size ({props.Size}) does not equal the expected content size ({Default.ByteContent.Length}).");
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_Returns_True_If_File_Exists()
        {
            var file = await TestFolder.SetupFileAsync();
            Assert.IsTrue(await file.ExistsAsync());
        }

        [TestMethod]
        public async Task ExistsAsync_Returns_False_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            Assert.IsFalse(await file.ExistsAsync());
        }

        #endregion

        #region CopyAsync Tests

        [TestMethod]
        public async Task CopyAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => file.CopyAsync((StoragePath)null!));
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
            Assert.AreNotEqual(srcFile.GetParent()?.Path, dstFile.GetParent()?.Path);

            await Assert.That.ElementExistsAsync(srcFile);
            await Assert.That.ElementExistsAsync(dstFile);

            await Assert.That.FileHasContentAsync(srcFile, Default.TextContent);
            await Assert.That.FileHasContentAsync(dstFile, Default.TextContent);
        }

        [TestMethod]
        public async Task CopyAsync_Fail_Throws_IOException_If_File_Already_Exists()
        {
            var (src, dst) = await TestFolder.SetupTwoConflictingFilesAsync();

            await Assert.ThrowsExceptionAsync<IOException>(
                () => src.CopyAsync(dst.Path, NameCollisionOption.Fail)
            );
        }
        
        [TestMethod]
        public async Task CopyAsync_ReplaceExisting_Replaces_Existing_File()
        {
            var (src, dst) = await TestFolder.SetupTwoConflictingFilesAsync();
            await src.WriteTextAsync(Default.TextContent);

            await src.CopyAsync(dst.Path, NameCollisionOption.ReplaceExisting);
            await Assert.That.FileHasContentAsync(dst, Default.TextContent);
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Throws_IOException_If_Source_Equals_Destination(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync();

            await Assert.ThrowsExceptionAsync<IOException>(
                () => srcFile.CopyAsync(srcFile.Path, options)
            );
        }
        
        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Throws_DirectoryNotFoundException_If_Source_Parent_Directory_Does_Not_Exist(NameCollisionOption options)
        {
            var srcFile = TestFolder.GetFolder(Default.SrcFolderName).GetFile(Default.SrcFileName);
            var dstDir = await TestFolder.SetupFolderAsync(basePath => basePath / Default.DstFolderName);
            var dstPath = dstDir.Path / Default.DstFileName;

            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => srcFile.CopyAsync(dstPath, options)
            );
        }
        
        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_Throws_DirectoryNotFoundException_If_Destination_Directory_Does_Not_Exist(NameCollisionOption options)
        {
            var src = await TestFolder.SetupFileAsync(basePath => basePath / Default.SrcFolderName / Default.FileName);
            var dstDir = TestFolder.GetFolder(Default.DstFolderName);
            var dstPath = dstDir.Path / Default.DstFileName;

            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => src.CopyAsync(dstPath, options)
            );
        }
        
        #endregion
        
        #region ReadTextAsync / WriteTextAsync Tests

        [TestMethod]
        public async Task WriteTextAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => file.WriteTextAsync(null!));
        }

        [TestMethod]
        public async Task Can_Read_Write_Text_With_Default_Encoding()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent);

            var content = await file.ReadTextAsync();
            Assert.AreEqual(Default.TextContent, content);
        }

        [TestMethod]
        public async Task Can_Read_Write_Text_With_Specified_Encoding()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteTextAsync(Default.TextContent, Encoding.ASCII);

            var content = await file.ReadTextAsync(Encoding.ASCII);
            Assert.AreEqual(Default.TextContent, content);
        }

        [TestMethod]
        public async Task WriteTextAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                () => file.WriteTextAsync(Default.TextContent)
            );
        }
        
        [TestMethod]
        public async Task ReadTextAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                () => file.ReadTextAsync()
            );
        }
        
        [TestMethod]
        public async Task WriteTextAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => file.WriteTextAsync(Default.TextContent)
            );
        }
        
        [TestMethod]
        public async Task ReadTextAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => file.ReadTextAsync()
            );
        }

        #endregion

        #region ReadBytesAsync / WriteBytesAsync Tests

        [TestMethod]
        public async Task WriteBytesAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => file.WriteBytesAsync(null!));
        }

        [TestMethod]
        public async Task Can_Read_Write_Bytes()
        {
            var file = await TestFolder.SetupFileAsync();
            await file.WriteBytesAsync(Default.ByteContent);

            var content = await file.ReadBytesAsync();
            CollectionAssert.AreEqual(Default.ByteContent, content);
        }

        [TestMethod]
        public async Task WriteBytesAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                () => file.WriteBytesAsync(Default.ByteContent)
            );
        }

        [TestMethod]
        public async Task ReadBytesAsync_Throws_FileNotFoundException_If_File_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(
                () => file.ReadBytesAsync()
            );
        }

        [TestMethod]
        public async Task WriteBytesAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => file.WriteBytesAsync(Default.ByteContent)
            );
        }

        [TestMethod]
        public async Task ReadBytesAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFolder(Default.FolderName).GetFile(Default.FileName);
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(
                () => file.ReadBytesAsync()
            );
        }

        #endregion

    }

}
