namespace Files.Specification.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Files.Specification.Tests.Assertions;
    using Files.Specification.Tests.Attributes;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Shouldly;

    public abstract class StorageFolderSpecificationTests : FileSystemTestBase
    {
        private char[] InvalidNewNameChars =>
            FileSystem.PathInformation.DirectorySeparatorChars
                .Append(FileSystem.PathInformation.VolumeSeparatorChar)
                .Distinct()
                .ToArray();

        public abstract IEnumerable<object[]> InvalidPathsData { get; }

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

        #region Parent Tests

        [TestMethod]
        public void Parent_StandardFolder_ReturnsParentFolder()
        {
            var expectedParent = TestFolder;
            var actualParent = TestFolder.GetFolder(Default.FolderName).Parent;
            actualParent?.Path.ShouldBeEffectivelyEqualTo(expectedParent.Path);
        }

        [TestMethod]
        public void Parent_FolderInRootFolder_ReturnsRootFolder()
        {
            var expectedParent = RootFolder;
            var actualParent = RootFolder.GetFolder(Default.FolderName).Parent;
            actualParent?.Path.ShouldBeEffectivelyEqualTo(expectedParent.Path);
        }

        [TestMethod]
        public void Parent_RootFolder_ReturnsNull()
        {
            RootFolder.Parent.ShouldBeNull();
        }

        #endregion

        #region GetFile Tests

        public IEnumerable<object[]> GetFileAllowedNamesData => new[]
        {
            new[] { "" },
            new[] { Default.FileName },
            new[] { FileSystem.PathInformation.CurrentDirectorySegment },
            new[] { FileSystem.PathInformation.ParentDirectorySegment },
            new[] { Default.FolderName + FileSystem.PathInformation.DirectorySeparatorChar + Default.FileName },
            new[]
            {
                FileSystem.PathInformation.ParentDirectorySegment +
                FileSystem.PathInformation.DirectorySeparatorChar +
                Default.FileName
            },
        };

        [TestMethod]
        public void GetFile_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => TestFolder.GetFile(null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(GetFileAllowedNamesData))]
        public void GetFile_StandardFolder_ReturnsFile(string fileName)
        {
            // Get a file in another folder for relative parent segment tests.
            var parentFolder = TestFolder.GetFolder(Default.FolderName);
            var file = parentFolder.GetFile(fileName);
            var expectedPath = parentFolder.Path / fileName;
            file.Path.ShouldBeEffectivelyEqualTo(expectedPath);
        }

        #endregion

        #region GetFolder Tests

        public IEnumerable<object[]> GetFolderAllowedNamesData => new[]
        {
            new[] { "" },
            new[] { Default.FileName },
            new[] { FileSystem.PathInformation.CurrentDirectorySegment },
            new[] { FileSystem.PathInformation.ParentDirectorySegment },
            new[] { Default.FolderName + FileSystem.PathInformation.DirectorySeparatorChar + Default.FileName },
            new[]
            {
                FileSystem.PathInformation.ParentDirectorySegment +
                FileSystem.PathInformation.DirectorySeparatorChar +
                Default.FileName
            },
        };

        [TestMethod]
        public void GetFolder_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => TestFolder.GetFolder(null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(GetFileAllowedNamesData))]
        public void GetFolder_StandardFolder_ReturnsFolder(string fileName)
        {
            // Get a folder in another folder for relative parent segment tests.
            var parentFolder = TestFolder.GetFolder(Default.FolderName);
            var file = parentFolder.GetFile(fileName);
            var expectedPath = parentFolder.Path / fileName;
            file.Path.ShouldBeEffectivelyEqualTo(expectedPath);
        }

        #endregion

        #region GetAllFilesAsync Tests

        [TestMethod]
        public async Task GetAllFilesAsync_ExistingFolder_ReturnsContainedFiles()
        {
            await TestFolder.SetupFileAsync($"1 {Default.FileName}");
            await TestFolder.SetupFileAsync($"2 {Default.FileName}");
            await TestFolder.SetupFileAsync(Default.FolderName, $"3 {Default.FileName}");

            var files = await TestFolder.GetAllFilesAsync();
            files.ShouldContain(file => file.Path.Name == $"1 {Default.FileName}");
            files.ShouldContain(file => file.Path.Name == $"2 {Default.FileName}");
            files.ShouldNotContain(file => file.Path.Name == $"3 {Default.FileName}");
        }

        [TestMethod]
        public async Task GetAllFilesAsync_EmptyFolder_ReturnsEmptyEnumerable()
        {
            var files = await TestFolder.GetAllFilesAsync();
            files.ShouldBeEmpty();
        }

        [TestMethod]
        public async Task GetAllFilesAsync_NonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAllFilesAsync());
        }
        
        [TestMethod]
        public async Task GetAllFilesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAllFilesAsync());
        }

        [TestMethod]
        public async Task GetAllFilesAsync_ConflictingFileExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.GetAllFilesAsync());
        }

        #endregion

        #region GetAllFoldersAsync Tests

        [TestMethod]
        public async Task GetAllFoldersAsync_ExistingFolder_ReturnsContainedFolders()
        {
            await TestFolder.SetupFolderAsync($"1 {Default.FolderName}");
            await TestFolder.SetupFolderAsync($"2 {Default.FolderName}");
            await TestFolder.SetupFolderAsync(Default.FolderName, $"3 {Default.FolderName}");
            await TestFolder.SetupFileAsync(Default.FileName);

            var folders = await TestFolder.GetAllFoldersAsync();
            folders.ShouldContain(folder => folder.Path.Name == Default.FolderName);
            folders.ShouldContain(folder => folder.Path.Name == $"2 {Default.FolderName}");
            folders.ShouldContain(folder => folder.Path.Name == $"2 {Default.FolderName}");
            folders.ShouldNotContain(folder => folder.Path.Name == $"3 {Default.FolderName}");
        }

        [TestMethod]
        public async Task GetAllFoldersAsync_EmptyFolder_ReturnsEmptyEnumerable()
        {
            var folders = await TestFolder.GetAllFoldersAsync();
            folders.ShouldBeEmpty();
        }

        [TestMethod]
        public async Task GetAllFoldersAsync_NonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAllFoldersAsync());
        }

        [TestMethod]
        public async Task GetAllFoldersAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAllFoldersAsync());
        }

        [TestMethod]
        public async Task GetAllFoldersAsync_ConflictingFileExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.GetAllFoldersAsync());
        }

        #endregion

        #region GetAllChildrenAsync Tests

        [TestMethod]
        public async Task GetAllChildrenAsync_ExistingFolder_ReturnsContainedChildren()
        {
            await TestFolder.SetupFileAsync($"1 {Default.FileName}");
            await TestFolder.SetupFileAsync($"2 {Default.FileName}");
            await TestFolder.SetupFileAsync(Default.FolderName, $"3 {Default.FileName}");
            await TestFolder.SetupFolderAsync($"1 {Default.FolderName}");
            await TestFolder.SetupFolderAsync($"2 {Default.FolderName}");
            await TestFolder.SetupFolderAsync(Default.FolderName, $"3 {Default.FolderName}");
            await TestFolder.SetupFileAsync(Default.FileName);

            var children = await TestFolder.GetAllChildrenAsync();
            children.ShouldContain(element => element.Path.Name == $"1 {Default.FileName}");
            children.ShouldContain(element => element.Path.Name == $"2 {Default.FileName}");
            children.ShouldContain(element => element.Path.Name == Default.FolderName);
            children.ShouldContain(element => element.Path.Name == $"2 {Default.FolderName}");
            children.ShouldContain(element => element.Path.Name == $"2 {Default.FolderName}");
            children.ShouldNotContain(element => element.Path.Name == $"3 {Default.FileName}");
            children.ShouldNotContain(element => element.Path.Name == $"3 {Default.FolderName}");
        }

        [TestMethod]
        public async Task GetAllChildrenAsync_EmptyFolder_ReturnsEmptyEnumerable()
        {
            var folders = await TestFolder.GetAllChildrenAsync();
            folders.ShouldBeEmpty();
        }

        [TestMethod]
        public async Task GetAllChildrenAsync_NonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAllChildrenAsync());
        }

        [TestMethod]
        public async Task GetAllChildrenAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAllChildrenAsync());
        }

        [TestMethod]
        public async Task GetAllChildrenAsync_ConflictingFileExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.GetAllChildrenAsync());
        }

        #endregion

        #region GetAttributesAsync Tests

        [TestMethod]
        public async Task GetAttributesAsync_ExistingFolder_DoesNotThrow()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.GetAttributesAsync();
            // Should not throw. We cannot know which attributes are present.
        }

        [TestMethod]
        public async Task GetAttributesAsync_NonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetAttributesAsync());
        }

        [TestMethod]
        public async Task GetAttributesAsync_ConflictingFileExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.GetAttributesAsync());
        }

        #endregion

        #region SetAttributesAsync Tests

        [TestMethod]
        public async Task SetAttributesAsync_InvalidFileAttributes_ThrowsArgumentException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FileName);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.SetAttributesAsync(Default.InvalidFileAttributes));
        }

        [TestMethod]
        public async Task SetAttributesAsync_ExistingFolder_DoesNotThrow()
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
        public async Task SetAttributesAsync_NonExistingFolder_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.SetAttributesAsync(FileAttributes.Normal));
        }

        [TestMethod]
        public async Task SetAttributesAsync_NonExistingParent_ThrowsDirectoryNotFoundException()
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.SetAttributesAsync(FileAttributes.Normal));
        }

        [TestMethod]
        public async Task SetAttributesAsync_ConflictingFileExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
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
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
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
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.GetPropertiesAsync());
        }

        [TestMethod]
        public async Task GetPropertiesAsync_ConflictingFileExistsAtLocation_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.GetPropertiesAsync());
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_InvalidOptions_ThrowsArgumentException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.CreateAsync(Default.InvalidCreationCollisionOption));
        }

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
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await folder.CreateAsync(recursive: true, options: options);
            await folder.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_NotRecursiveAndNonExistingParent_ThrowsDirectoryNotFoundException(CreationCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
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
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.CreateAsync(recursive, options));
        }

        [DataTestMethod]
        [DataRow(CreationCollisionOption.Fail)]
        [DataRow(CreationCollisionOption.ReplaceExisting)]
        [DataRow(CreationCollisionOption.Ignore)]
        public async Task CreateAsync_RecursiveAndConflictingFileExistsAtParentLocation_ThrowsIOException(CreationCollisionOption options)
        {
            var parentFolder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            var thisFolder = parentFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<IOException>(async () => await thisFolder.CreateAsync(recursive: true, options));
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_InvalidOptions_ThrowsArgumentException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.DeleteAsync(Default.InvalidDeletionOption));
        }

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
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.DeleteAsync(DeletionOption.Fail));
        }

        [TestMethod]
        public async Task DeleteAsync_IgnoreMissingAndNonExistingFolder_DoesNothing()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await folder.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [TestMethod]
        public async Task DeleteAsync_IgnoreMissingAndNonExistingParent_DoesNothing()
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await folder.DeleteAsync(DeletionOption.IgnoreMissing);
        }

        [DataTestMethod]
        [DataRow(DeletionOption.Fail)]
        [DataRow(DeletionOption.IgnoreMissing)]
        public async Task DeleteAsync_ConflictingFileExistsAtLocation_ThrowsIOException(DeletionOption options)
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.DeleteAsync(options));
        }

        #endregion

        #region CopyAsync Tests

        [TestMethod]
        public async Task CopyAsync_NullParameters_ThrowsArgumentNullException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await folder.CopyAsync(destinationPath: null!));
        }

        [TestMethod]
        public async Task CopyAsync_InvalidOptions_ThrowsArgumentException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FileName);
            var dst = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.CopyAsync(dst, Default.InvalidNameCollisionOption));
        }

        [TestMethod]
        public async Task CopyAsync_ValidForeignFileSystemPath_UsesForeignFileSystemPath()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var dstFolder = TestFolder.GetFolder(Default.DstFolderSegments);
            var foreignDstPathMock = new Mock<StoragePath>(dstFolder.Path.ToString()) { CallBase = true };
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            await folder.CopyAsync(foreignDstPathMock.Object);
            await dstFolder.ShouldExistAsync();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(InvalidPathsData))]
        public async Task CopyAsync_InvalidForeignFileSystemPath_ThrowsArgumentException(string invalidPathString)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            var foreignDstPathMock = new Mock<StoragePath>(invalidPathString) { CallBase = true };
            await Should.ThrowAsync<ArgumentException>(async () => await folder.CopyAsync(foreignDstPathMock.Object));
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ExistingFolderAndExistingDestinationFolder_CopiesFolder(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var dstParentFolder = await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);

            var dstFolder = await srcFolder.CopyAsync(dstFolderPath, options);

            dstFolder.Parent?.Path.ShouldBeEffectivelyEqualTo(dstParentFolder.Path);
            await srcFolder.ShouldExistAsync();
            await dstFolder.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ExistingFolderAndExistingDestinationFolder_CopiesFolderContents(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);

            // Simply setup several child elements. Try be as varied as possible.
            // Top level file with contents.
            var srcTopLevelFile = await srcFolder.SetupFileAsync(Default.FileName);
            await srcTopLevelFile.WriteTextAsync(Default.TextContent);

            // Empty top level folder.
            await srcFolder.SetupFolderAsync(Default.FolderName);

            // Top level folder with contents.
            var srcTopLevelFolder = await srcFolder.SetupFolderAsync(Default.SrcParentFolderName);
            await srcTopLevelFolder.SetupFolderAsync(Default.FolderName);
            var srcNestedFile = await srcTopLevelFolder.SetupFileAsync(Default.FileName);
            await srcNestedFile.WriteTextAsync(Default.TextContent);

            // Copy and retrieve contents for asserts.
            var dstFolder = await srcFolder.CopyAsync(dstFolderPath, options);
            var dstTopLevelFile = dstFolder.GetFile(Default.FileName);
            var dstEmptyTopLevelFolder = dstFolder.GetFolder(Default.FolderName);
            var dstTopLevelFolder = dstFolder.GetFolder(Default.SrcParentFolderName);
            var dstNestedFolder = dstTopLevelFolder.GetFolder(Default.FolderName);
            var dstNestedFile = dstTopLevelFolder.GetFile(Default.FileName);

            await dstFolder.ShouldExistAsync();
            await dstTopLevelFile.ShouldExistAsync();
            await dstEmptyTopLevelFolder.ShouldExistAsync();
            await dstTopLevelFolder.ShouldExistAsync();
            await dstNestedFolder.ShouldExistAsync();
            await dstNestedFile.ShouldExistAsync();
            await dstTopLevelFile.ShouldHaveContentAsync(Default.TextContent);
            await dstNestedFile.ShouldHaveContentAsync(Default.TextContent);
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_NonExistingFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            var destination = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.CopyAsync(destination, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_NonExistingParent_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            var destination = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.CopyAsync(destination, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_NonExistingDestinationFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var destination = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await srcFolder.CopyAsync(destination, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ConflictingFileExistsAtSource_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var srcFolder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderInSrcSegments);
            var destination = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.CopyAsync(destination, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_ConflictingFileExistsAtDestination_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFileAsync(Default.SharedFileFolderInDstSegments);
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var destination = TestFolder.GetPath(Default.SharedFileFolderInDstSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.CopyAsync(destination, options));
        }

        [TestMethod]
        public async Task CopyAsync_FailAndExistingFolderAtDestination_ThrowsIOException()
        {
            var dstFolder = await TestFolder.SetupFolderAsync(Default.DstFolderSegments);
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.CopyAsync(dstFolder.Path, NameCollisionOption.Fail));
        }

        [TestMethod]
        public async Task CopyAsync_ReplaceExistingAndExistingFolderAtDestination_ReplacesExistingFolder()
        {
            // Folder to be replaced:
            // folder
            // |_ subFolder
            // |_ subFile (Content: "Initial content.")
            var folderToBeReplaced = await TestFolder.SetupFolderAsync(Default.DstFolderSegments);
            await folderToBeReplaced.SetupFolderAsync(Default.FolderName);
            var fileToBeReplaced = await folderToBeReplaced.SetupFileAsync(Default.FileName);
            await fileToBeReplaced.WriteTextAsync("Initial content.");

            // New folder:
            // folder
            // |_ subFile (Content: "New content.")
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var newFile = await srcFolder.SetupFileAsync(Default.FileName);
            await newFile.WriteTextAsync("New content.");

            var dstFolder = await srcFolder.CopyAsync(folderToBeReplaced.Path, NameCollisionOption.ReplaceExisting);

            await dstFolder.ShouldExistAsync();
            await dstFolder.GetFolder(Default.FolderName).ShouldNotExistAsync();
            await dstFolder.GetFile(Default.FileName).ShouldExistAsync();
            await dstFolder.GetFile(Default.FileName).ShouldHaveContentAsync("New content.");
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task CopyAsync_CopyToSameLocation_ThrowsIOException(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var destination = srcFolder.Path;
            await Should.ThrowAsync<IOException>(async () => await srcFolder.CopyAsync(destination, options));
            await srcFolder.ShouldExistAsync();
        }

        #endregion

        #region MoveAsync Tests

        [TestMethod]
        public async Task MoveAsync_NullParameters_ThrowsArgumentNullException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await folder.MoveAsync(destinationPath: null!));
        }

        [TestMethod]
        public async Task MoveAsync_InvalidOptions_ThrowsArgumentException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var dst = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.MoveAsync(dst, Default.InvalidNameCollisionOption));
        }

        [TestMethod]
        public async Task MoveAsync_ValidForeignFileSystemPath_UsesForeignFileSystemPath()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var dstFolder = TestFolder.GetFolder(Default.DstFolderSegments);
            var foreignDstPathMock = new Mock<StoragePath>(dstFolder.Path.ToString()) { CallBase = true };
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            await folder.MoveAsync(foreignDstPathMock.Object);
            await dstFolder.ShouldExistAsync();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(InvalidPathsData))]
        public async Task MoveAsync_InvalidForeignFileSystemPath_ThrowsArgumentException(string invalidPathString)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            var foreignDstPathMock = new Mock<StoragePath>(invalidPathString) { CallBase = true };
            await Should.ThrowAsync<ArgumentException>(async () => await folder.MoveAsync(foreignDstPathMock.Object));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ExistingFolderAndExistingDestinationFolder_MovesFolder(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var dstParentFolder = await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);

            var dstFolder = await srcFolder.MoveAsync(dstFolderPath, options);

            dstFolder.Parent?.Path.ShouldBeEffectivelyEqualTo(dstParentFolder.Path);
            await srcFolder.ShouldNotExistAsync();
            await dstFolder.ShouldExistAsync();
        }

        [DataTestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ExistingFolderAndExistingDestinationFolder_MovesFolderContents(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);

            // Simply setup several child elements. Try be as varied as possible.
            // Top level file with contents.
            var srcTopLevelFile = await srcFolder.SetupFileAsync(Default.FileName);
            await srcTopLevelFile.WriteTextAsync(Default.TextContent);

            // Empty top level folder.
            await srcFolder.SetupFolderAsync(Default.FolderName);

            // Top level folder with contents.
            var srcTopLevelFolder = await srcFolder.SetupFolderAsync(Default.SrcParentFolderName);
            await srcTopLevelFolder.SetupFolderAsync(Default.FolderName);
            var srcNestedFile = await srcTopLevelFolder.SetupFileAsync(Default.FileName);
            await srcNestedFile.WriteTextAsync(Default.TextContent);

            // Copy and retrieve contents for asserts.
            var dstFolder = await srcFolder.MoveAsync(dstFolderPath, options);
            var dstTopLevelFile = dstFolder.GetFile(Default.FileName);
            var dstEmptyTopLevelFolder = dstFolder.GetFolder(Default.FolderName);
            var dstTopLevelFolder = dstFolder.GetFolder(Default.SrcParentFolderName);
            var dstNestedFolder = dstTopLevelFolder.GetFolder(Default.FolderName);
            var dstNestedFile = dstTopLevelFolder.GetFile(Default.FileName);

            await srcFolder.ShouldNotExistAsync();
            await dstFolder.ShouldExistAsync();
            await dstTopLevelFile.ShouldExistAsync();
            await dstEmptyTopLevelFolder.ShouldExistAsync();
            await dstTopLevelFolder.ShouldExistAsync();
            await dstNestedFolder.ShouldExistAsync();
            await dstNestedFile.ShouldExistAsync();
            await dstTopLevelFile.ShouldHaveContentAsync(Default.TextContent);
            await dstNestedFile.ShouldHaveContentAsync(Default.TextContent);
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_NonExistingFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.MoveAsync(dstFolderPath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_NonExistingParent_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.MoveAsync(dstFolderPath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_NonExistingDestinationFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await srcFolder.MoveAsync(dstFolderPath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ConflictingFileExistsAtSource_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var srcFolder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderInSrcSegments);
            var dstFolderPath = TestFolder.GetPath(Default.DstFolderSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.MoveAsync(dstFolderPath, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ConflictingFileExistsAtDestination_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFileAsync(Default.SharedFileFolderInDstSegments);
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var dstFolderPath = TestFolder.GetPath(Default.SharedFileFolderInDstSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.MoveAsync(dstFolderPath, options));
        }

        [TestMethod]
        public async Task MoveAsync_FailAndExistingFolderAtDestination_ThrowsIOException()
        {
            var dstFolder = await TestFolder.SetupFolderAsync(Default.DstFolderSegments);
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.MoveAsync(dstFolder.Path, NameCollisionOption.Fail));
        }

        [TestMethod]
        public async Task MoveAsync_ReplaceExistingAndExistingFolderAtDestination_ReplacesExistingFolder()
        {
            // Folder to be replaced:
            // folder
            // |_ subFolder
            // |_ subFile (Content: "Initial content.")
            var folderToBeReplaced = await TestFolder.SetupFolderAsync(Default.DstFolderSegments);
            await folderToBeReplaced.SetupFolderAsync(Default.FolderName);
            var fileToBeReplaced = await folderToBeReplaced.SetupFileAsync(Default.FileName);
            await fileToBeReplaced.WriteTextAsync("Initial content.");

            // New folder:
            // folder
            // |_ subFile (Content: "New content.")
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderSegments);
            var newFile = await srcFolder.SetupFileAsync(Default.FileName);
            await newFile.WriteTextAsync("New content.");

            var dstFolder = await srcFolder.MoveAsync(folderToBeReplaced.Path, NameCollisionOption.ReplaceExisting);

            await srcFolder.ShouldNotExistAsync();
            await dstFolder.ShouldExistAsync();
            await dstFolder.GetFolder(Default.FolderName).ShouldNotExistAsync();
            await dstFolder.GetFile(Default.FileName).ShouldExistAsync();
            await dstFolder.GetFile(Default.FileName).ShouldHaveContentAsync("New content.");
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_MoveToSameLocation_ThrowsIOException(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.MoveAsync(srcFolder.Path, options));
            await srcFolder.ShouldExistAsync();
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_MoveParentIntoSubFolder_ThrowsIOException(NameCollisionOption options)
        {
            var srcFolder = await TestFolder.SetupFolderAsync(Default.SrcFolderName);
            var dstFolder = await srcFolder.SetupFolderAsync(Default.DstFolderName);
            await Should.ThrowAsync<IOException>(async () => await srcFolder.MoveAsync(dstFolder.Path, options));
        }

        #endregion

        #region RenameAsync Tests

        public IEnumerable<object[]> RenameAsyncInvalidNewNamesData
        {
            get
            {
                yield return new[] { "" };

                foreach (var invalidName in InvalidNewNameChars.Select(invalidChar => invalidChar + Default.FolderName))
                {
                    yield return new[] { invalidName };
                }
            }
        }

        [TestMethod]
        public async Task RenameAsync_NullParameters_ThrowsArgumentNullException()
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<ArgumentNullException>(async () => await folder.RenameAsync(newName: null!));
        }

        [TestMethod]
        public async Task RenameAsync_InvalidOptions_ThrowsArgumentException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.RenameAsync(Default.RenamedFolderName, Default.InvalidNameCollisionOption));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(RenameAsyncInvalidNewNamesData))]
        public async Task RenameAsync_InvalidName_ThrowsArgumentException(string newName)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<ArgumentException>(async () => await folder.RenameAsync(newName));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_ExistingFolder_RenamesFolder(NameCollisionOption options)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await folder.SetupFileAsync(Default.FileName);

            var renamed = await folder.RenameAsync(Default.RenamedFolderName, options);

            renamed.Path.ToString().ShouldEndWith(Default.RenamedFolderName);
            await folder.ShouldNotExistAsync();
            await renamed.ShouldExistAsync();
            await renamed.GetFile(Default.FileName).ShouldExistAsync();
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_NonExistingFolder_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderName);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.RenameAsync(Default.RenamedFolderName, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_NonExistingParent_ThrowsDirectoryNotFoundException(NameCollisionOption options)
        {
            var folder = TestFolder.GetFolder(Default.FolderWithNonExistingParentSegments);
            await Should.ThrowAsync<DirectoryNotFoundException>(async () => await folder.RenameAsync(Default.RenamedFolderName, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_ConflictingFileExistsAtLocation_ThrowsIOException(NameCollisionOption options)
        {
            var folder = await TestFolder.SetupFileAndGetFolderAtSameLocation(Default.SharedFileFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.RenameAsync(Default.RenamedFolderName, options));
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task MoveAsync_ConflictingFileExistsAtRenameDestination_ThrowsIOException(NameCollisionOption options)
        {
            await TestFolder.SetupFileAsync(Default.SharedFileFolderName);
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.RenameAsync(Default.SharedFileFolderName, options));
        }

        [TestMethod]
        public async Task RenameAsync_FailAndExistingFolderAtRenameDestination_ThrowsIOException()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await TestFolder.SetupFolderAsync(Default.RenamedFolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.RenameAsync(Default.RenamedFolderName, NameCollisionOption.Fail));
        }

        [TestMethod]
        public async Task RenameAsync_ReplaceExistingAndExistingFolderAtRenameDestination_ReplacesExistingFolder()
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var conflictingFolder = await TestFolder.SetupFolderAsync(Default.RenamedFolderName);
            await conflictingFolder.SetupFileAsync(Default.FileName);

            await folder.RenameAsync(Default.RenamedFolderName, NameCollisionOption.ReplaceExisting);

            await folder.ShouldNotExistAsync();
            await conflictingFolder.GetFile(Default.FileName).ShouldNotExistAsync();
        }

        [TestMethod]
        [DataRow(NameCollisionOption.Fail)]
        [DataRow(NameCollisionOption.ReplaceExisting)]
        public async Task RenameAsync_SameName_ThrowsIOException(NameCollisionOption options)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            await Should.ThrowAsync<IOException>(async () => await folder.RenameAsync(Default.FolderName, options));
            await folder.ShouldExistAsync();
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
