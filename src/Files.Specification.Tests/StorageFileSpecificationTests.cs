﻿namespace Files.Specification.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Files.Specification.Tests.Assertions;
    using Files.Specification.Tests.Attributes;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public abstract class StorageFileSpecificationTests : FileSystemTestBase
    {

        private char[] InvalidNewNameChars =>
            FileSystem.PathInformation.InvalidPathChars
                .Concat(FileSystem.PathInformation.InvalidFileNameChars)
                .Concat(FileSystem.PathInformation.DirectorySeparatorChars)
                .Append(FileSystem.PathInformation.VolumeSeparatorChar)
                .Distinct()
                .ToArray();

        public StorageFileSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region FileSystem Tests

        [TestMethod]
        public void FileSystem_StandardFile_IsSameInstanceAsOfParentFolder()
        {
            // This is obviously not the best test case, as the file system could be a different
            // instance in a lot of other scenarios.
            // Testing all of them is incredibly tedious with little value gained though, so
            // I trust on common sense of library implementers (including myself) here.
            // I might regret that.
            var file = TestFolder.GetFile(Default.FileName);
            file.FileSystem.ShouldBeSameAs(TestFolder.FileSystem);
        }

        #endregion

        #region Path Tests

        [TestMethod]
        public void Path_StandardFile_ReturnsExpectedPath()
        {
            // This test doesn't test that the property is ALWAYS implemented correctly.
            // To do that, we'd have run this test after each method which creates a new instance (e.g.
            // after calling MoveAsync, CopyAsync, GetFile, ...), because the value can always be set
            // wrongly there.
            // That is way beyond the scope of this specification though.
            var file = TestFolder.GetFile(Default.FileName);
            var parentPath = TestFolder.Path;
            var expectedPath = parentPath.Join(Default.FileName);
            file.Path.ShouldBeEffectivelyEqualTo(expectedPath);
        }

        #endregion

        #region GetParent Tests

        [TestMethod]
        public void GetParent_StandardFile_ReturnsParentFolder()
        {
            var expectedParent = TestFolder;
            var actualParent = TestFolder.GetFile(Default.FileName).GetParent();
            actualParent.Path.ShouldBeEffectivelyEqualTo(expectedParent.Path);
        }

        [TestMethod]
        public void GetParent_FileInRootFolder_ReturnsRootFolder()
        {
            var expectedParent = RootFolder;
            var actualParent = RootFolder.GetFile(Default.FileName).GetParent();
            actualParent.Path.ShouldBeEffectivelyEqualTo(expectedParent.Path);
        }

        #endregion

        #region GetAttributesAsync Tests

        [TestMethod]
        public async Task GetAttributesAsync_ExistingFile_DoesNotThrow()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await file.GetAttributesAsync());
        }

        #endregion

        #region SetAttributesAsync Tests

        [TestMethod]
        public async Task SetAttributesAsync_ExistingFile_DoesNotThrow()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await file.SetAttributesAsync(FileAttributes.Normal);
            // Should not throw. We cannot know which attributes can be set.
        }

        [TestMethod]
        public async Task SetAttributes_InvalidAttributeCombination_DoesNotThrow()
        {
            // Of course, there is no guarantee that this is invalid in every FS implementation.
            // But in most, it should be.
            var invalidAttributes = FileAttributes.Normal | FileAttributes.Directory;
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await file.SetAttributesAsync(invalidAttributes);
        }

        [TestMethod]
        public async Task SetAttributesAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.SetAttributesAsync(FileAttributes.Normal));
        }

        [TestMethod]
        public async Task SetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.SetAttributesAsync(FileAttributes.Normal));
        }

        [TestMethod]
        public async Task SetAttributesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await file.SetAttributesAsync(FileAttributes.Normal));
        }

        #endregion

        #region GetPropertiesAsync Tests

        [TestMethod]
        public async Task GetPropertiesAsync_ExistingFile_ReturnsNonNullProperties()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            var props = await file.GetPropertiesAsync();
            props.ShouldNotBeNull();
            // Without any additional info, this is the best we can do.
        }

        [DataTestMethod]
        [DataRow("file", "ext")]
        [DataRow("fileWithoutExt", null)]
        [DataRow("file{0}with{0}many{0}extensions{0}", "finalExt")]
        public async Task GetPropertiesAsync_ExistingFile_ReturnsPropertiesWithExpectedValues(string name, string? extension)
        {
            name = string.Format(name, FileSystem.PathInformation.ExtensionSeparatorChar);
            var extSeparator = extension is null ? (char?)null : FileSystem.PathInformation.ExtensionSeparatorChar;
            var fullName = $"{name}{extSeparator}{extension}";

            var file = await TestFolder.SetupFileAsync(fullName);
            await file.WriteBytesAsync(Default.ByteContent);
            var props = await file.GetPropertiesAsync();

            // Test the props to the best of our abilities. Specialities:
            // - We cannot really test that the name returns the REAL file name, because we don't know if
            //   the FS uses case sensitive paths.
            // - Not testing that ModifiedOn is null without any modification. We don't know if the FS leaves it blank
            //   or sets it on creation.
            // - Testing dates is somewhat hard. We only ensure that they are in a certain time range relative to now.
            var timeSinceCreation = DateTimeOffset.UtcNow - props.CreatedOn;
            var timeSinceModification = DateTimeOffset.UtcNow - props.ModifiedOn;

            props.Name.ShouldBe(fullName);
            props.NameWithoutExtension.ShouldBe(name);
            props.Extension.ShouldBe(extension);
            props.Size.ShouldBe((ulong)Default.ByteContent.Length);
            timeSinceCreation.ShouldBeLessThan(TimeSpan.FromSeconds(10));
            timeSinceModification?.ShouldBeLessThan(TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public async Task GetPropertiesAsync_NonExistingFile_ThrowsFileNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.GetPropertiesAsync());
        }

        [TestMethod]
        public async Task GetPropertiesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.GetPropertiesAsync());
        }

        [TestMethod]
        public async Task GetPropertiesAsync_ConflictingFolderExistsAtLocation_ThrowsIOException()
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await file.GetPropertiesAsync());
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_ExistingFile_ReturnsTrue()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await file.ShouldNotExistAsync();
        }

        [TestMethod]
        public async Task ExistsAsync_ConflictingFolderExistsAtLocation_ReturnsFalse()
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await file.ShouldNotExistAsync();
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
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await file.CreateAsync(recursive: true, options: options);
            await file.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_NotRecursiveAndNonExistingParent_ThrowsDirectoryNotFoundException(CreationCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.CreateAsync(recursive: false, options));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_FailAndExistingFile_ThrowsIOException(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await Should.ThrowAsync<IOException>(async () => await file.CreateAsync(recursive, CreationCollisionOption.Fail));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreateAsync_ReplaceExistingAndExistingFile_ReplacesFile(bool recursive)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await file.CreateAsync(recursive, options));
        }

        #endregion

        #region DeleteAsync Tests

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ExistingFile_DeletesFile(DeletionOption options)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
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
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await file.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ConflictingFolderExistsAtLocation_ThrowsIOException(DeletionOption options)
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await file.DeleteAsync(options));
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndConflictingFileExistsAtParentLocation_ThrowsIOException(CreationCollisionOption options)
        {
            var parentFolder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            var thisFile = parentFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<IOException>(async () => await thisFile.CreateAsync(recursive: true, options));
        }

        #endregion

        #region CopyAsync Tests

        [TestMethod]
        public async Task CopyAsync_NullParameters_ThrowsArgumentNullException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.CopyAsync(destinationPath: null!));
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ExistingFileAndExistingDestinationFolder_CopiesFile(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstParentFolder = await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await srcFile.WriteTextAsync(Default.TextContent);

            var dstFile = await srcFile.CopyAsync(dstFilePath, options);

            dstFile.GetParent().Path.ShouldBeEffectivelyEqualTo(dstParentFolder.Path);
            await srcFile.ShouldExistAsync();
            await dstFile.ShouldExistAsync();
            await srcFile.ShouldHaveContentAsync(Default.TextContent);
            await dstFile.ShouldHaveContentAsync(Default.TextContent);
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_NonExistingFile_ThrowsFileNotFoundException(NameCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileName);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.CopyAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_NonExistingParent_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.CopyAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_NonExistingDestinationFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await srcFile.CopyAsync(dstFilePath, options));
        }
        
        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ConflictingFolderExistsAtSource_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var srcFile = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderInSrcSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFile.CopyAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ConflictingFolderExistsAtDestination_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.SharedFileFolderInDstSegments);
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstFilePath = TestFolder.GetPath(Default.SharedFileFolderInDstSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFile.CopyAsync(dstFilePath, options));
        }

        [TestMethod]
        public async Task CopyAsync_FailAndExistingFileAtDestination_ThrowsIOException()
        {
            var dstFile = await TestFolder.SetupFileAsync(Default.DstFileSegments);
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFile.CopyAsync(dstFile.Path, NameCollisionOption.Fail));
        }
        
        [TestMethod]
        public async Task CopyAsync_ReplaceExistingAndExistingFileAtDestination_ReplacesExistingFile()
        {
            await TestFolder.SetupFileAsync(Default.DstFileSegments);
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await srcFile.WriteBytesAsync(Default.ByteContent);
            var dstFile = await srcFile.CopyAsync(dstFilePath, NameCollisionOption.ReplaceExisting);
            await dstFile.ShouldHaveContentAsync(Default.ByteContent);
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_CopyToSameLocation_ThrowsIOException(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync(Default.FileName);
            var dstFilePath = srcFile.Path;
            await Should.ThrowAsync<IOException>(async () => await srcFile.CopyAsync(dstFilePath, options));
        }

        #endregion

        #region MoveAsync Tests

        [TestMethod]
        public async Task MoveAsync_NullParameters_ThrowsArgumentNullException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.MoveAsync(destinationPath: null!));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ExistingFileAndExistingDestinationFolder_MovesFile(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstParentFolder = await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await srcFile.WriteTextAsync(Default.TextContent);

            var dstFile = await srcFile.MoveAsync(dstFilePath, options);

            dstFile.GetParent().Path.ShouldBeEffectivelyEqualTo(dstParentFolder.Path);
            await srcFile.ShouldNotExistAsync();
            await dstFile.ShouldExistAsync();
            await dstFile.ShouldHaveContentAsync(Default.TextContent);
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_NonExistingFile_ThrowsFileNotFoundException(NameCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileName);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.MoveAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_NonExistingParent_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.MoveAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_NonExistingDestinationFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await srcFile.MoveAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ConflictingFolderExistsAtSource_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var srcFile = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderInSrcSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFile.MoveAsync(dstFilePath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ConflictingFolderExistsAtDestination_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.SharedFileFolderInDstSegments);
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstFilePath = TestFolder.GetPath(Default.SharedFileFolderInDstSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFile.MoveAsync(dstFilePath, options));
        }

        [TestMethod]
        public async Task MoveAsync_FailAndExistingFileAtDestination_ThrowsIOException()
        {
            var dstFile = await TestFolder.SetupFileAsync(Default.DstFileSegments);
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFile.MoveAsync(dstFile.Path, NameCollisionOption.Fail));
        }

        [TestMethod]
        public async Task MoveAsync_ReplaceExistingAndExistingFileAtDestination_ReplacesExistingFile()
        {
            await TestFolder.SetupFileAsync(Default.DstFileSegments);
            var srcFile = await TestFolder.SetupFileAsync(Default.SrcFileSegments);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);
            await srcFile.WriteBytesAsync(Default.ByteContent);
            var dstFile = await srcFile.MoveAsync(dstFilePath, NameCollisionOption.ReplaceExisting);
            await dstFile.ShouldHaveContentAsync(Default.ByteContent);
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_MoveToSameLocation_DoesNothing(NameCollisionOption options)
        {
            var srcFile = await TestFolder.SetupFileAsync(Default.FileName);
            await srcFile.MoveAsync(srcFile.Path, options);
            await srcFile.ShouldExistAsync();
        }

        #endregion

        #region RenameAsync Tests

        public IEnumerable<object[]> RenameAsyncInvalidNewNamesData
        {
            get
            {
                yield return new[] { "" };

                foreach (var invalidName in InvalidNewNameChars.Select(invalidChar => invalidChar + Default.FileName))
                {
                    yield return new[] { invalidName };
                }
            }
        }

        [TestMethod]
        public async Task RenameAsync_NullParameters_ThrowsArgumentNullException()
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.RenameAsync(newName: null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(RenameAsyncInvalidNewNamesData))]
        public async Task RenameAsync_InvalidName_ThrowsArgumentException(string newName)
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<ArgumentException>(async () => await file.RenameAsync(newName));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_ExistingFile_RenamesFile(NameCollisionOption options)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await file.WriteTextAsync(Default.TextContent);

            var renamed = await file.RenameAsync(Default.RenamedFileName, options);

            await file.ShouldNotExistAsync();
            await renamed.ShouldExistAsync();
            await renamed.ShouldHaveContentAsync(Default.TextContent);
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_NonExistingFile_ThrowsFileNotFoundException(NameCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () => await file.RenameAsync(Default.RenamedFileName, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_NonExistingParent_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.RenameAsync(Default.RenamedFileName, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_ConflictingFolderExistsAtLocation_ThrowsIOException(NameCollisionOption options)
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await file.RenameAsync(Default.RenamedFileName, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ConflictingFolderExistsAtRenameDestination_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.SharedFileFolderName);
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await Should.ThrowAsync<IOException>(async () => await file.RenameAsync(Default.SharedFileFolderName, options));
        }

        [TestMethod]
        public async Task RenameAsync_FailAndExistingFileAtRenameDestination_ThrowsIOException()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await TestFolder.SetupFileAsync(Default.RenamedFileName);
            await Should.ThrowAsync<IOException>(async () => await file.RenameAsync(Default.RenamedFileName, NameCollisionOption.Fail));
        }

        [TestMethod]
        public async Task RenameAsync_ReplaceExistingAndExistingFileAtRenameDestination_ReplacesExistingFile()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            var conflictingFile = await TestFolder.SetupFileAsync(Default.RenamedFileName);
            await conflictingFile.WriteTextAsync(Default.TextContent);

            await file.RenameAsync(Default.RenamedFileName, NameCollisionOption.ReplaceExisting);

            await file.ShouldNotExistAsync();
            await conflictingFile.ShouldHaveEmptyContentAsync();
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_SameName_DoesNothing(NameCollisionOption options)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await file.RenameAsync(Default.FileName, options);
            await file.ShouldExistAsync();
        }

        #endregion

        #region OpenAsync Tests

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task OpenAsync_ExistingFile_OpensStream(FileAccess fileAccess)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            using var stream = await file.OpenAsync(fileAccess);
            stream.ShouldNotBeNull();
        }

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task OpenAsync_ExistingFile_OpensStreamToCorrectFile(FileAccess fileAccess)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await file.WriteTextAsync(Default.TextContent);

            using var stream = await file.OpenAsync(fileAccess);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            content.ShouldBe(Default.TextContent);
        }

        [TestMethod]
        public async Task OpenAsync_ExistingFile_StreamCanBeUsedForReadingAndWriting()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            
            using (var writeStream = await file.OpenAsync())
            using (var streamWriter = new StreamWriter(writeStream))
            {
                await streamWriter.WriteAsync(Default.TextContent);
                await streamWriter.FlushAsync();
            }

            using (var readStream = await file.OpenAsync())
            using (var streamReader = new StreamReader(readStream))
            {
                var content = await streamReader.ReadToEndAsync();
                content.ShouldBe(Default.TextContent);
            }
        }

        [TestMethod]
        public async Task OpenAsync_ExistingFileAndReadFileAccess_ReturnsReadableStream()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            using var stream = await file.OpenAsync(FileAccess.Read);
            stream.CanRead.ShouldBeTrue();
        }
        
        [TestMethod]
        public async Task OpenAsync_ExistingFileAndWriteFileAccess_ReturnsWriteableStream()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            using var stream = await file.OpenAsync(FileAccess.Write);
            stream.CanWrite.ShouldBeTrue();
        }
        
        [TestMethod]
        public async Task OpenAsync_ExistingFileAndReadWriteFileAccess_ReturnsReadableAndWriteableStream()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            using var stream = await file.OpenAsync(FileAccess.ReadWrite);
            stream.CanRead.ShouldBeTrue();
            stream.CanWrite.ShouldBeTrue();
        }

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task OpenAsync_NonExistingFile_ThrowsFileNotFoundException(FileAccess fileAccess)
        {
            var file = TestFolder.GetFile(Default.FileName);
            await Should.ThrowAsync<FileNotFoundException>(async () =>
            {
                using var stream = await file.OpenAsync(fileAccess);
            });
        }

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task SetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException(FileAccess fileAccess)
        {
            var file = TestFolder.GetFile(Default.FileWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () =>
            {
                using var stream = await file.OpenAsync(fileAccess);
            });
        }

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task OpenAsync_ConflictingFolderExistsAtLocation_ThrowsIOException(FileAccess fileAccess)
        {
            var file = await TestFolder.SetupFolderAndGetFileAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () =>
            {
                using var stream = await file.OpenAsync(fileAccess);
            });
        }

        #endregion

        #region ReadTextAsync / WriteTextAsync Tests

        [TestMethod]
        public async Task WriteTextAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.WriteTextAsync(null!));
        }

        [TestMethod]
        public async Task Can_Read_Write_Text_With_Default_Encoding()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await file.WriteTextAsync(Default.TextContent);
            await file.ShouldHaveContentAsync(Default.TextContent);
        }

        [TestMethod]
        public async Task Can_Read_Write_Text_With_Specified_Encoding()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = TestFolder.GetFile(Default.FolderName, Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.WriteTextAsync(Default.TextContent));
        }
        
        [TestMethod]
        public async Task ReadTextAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FolderName, Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.ReadTextAsync());
        }

        #endregion

        #region ReadBytesAsync / WriteBytesAsync Tests

        [TestMethod]
        public async Task WriteBytesAsync_Throws_ArgumentNullException()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await file.WriteBytesAsync(null!));
        }

        [TestMethod]
        public async Task Can_Read_Write_Bytes()
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
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
            var file = TestFolder.GetFile(Default.FolderName, Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.WriteBytesAsync(Default.ByteContent));
        }

        [TestMethod]
        public async Task ReadBytesAsync_Throws_DirectoryNotFoundException_If_Parent_Does_Not_Exist()
        {
            var file = TestFolder.GetFile(Default.FolderName, Default.FileName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await file.ReadBytesAsync());
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsFullPathString()
        {
            var file = TestFolder.GetFile(Default.FileName);
            file.ToString().ShouldBe(file.Path.FullPath.ToString());
        }

        #endregion

    }

}
