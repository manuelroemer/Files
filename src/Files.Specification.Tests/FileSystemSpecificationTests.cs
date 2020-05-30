namespace Files.Specification.Tests
{
    using System;
    using System.Collections.Generic;
    using Files.Specification.Tests.Attributes;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public abstract class FileSystemSpecificationTests : FileSystemTestBase
    {
        public const KnownFolder UndefinedKnownFolder = (KnownFolder)int.MinValue;

        public abstract IEnumerable<object[]> KnownFolderData { get; }

        public abstract IEnumerable<object[]> ValidPathStringData { get; }

        public abstract IEnumerable<object[]> InvalidPathStringData { get; }

        public FileSystemSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region PathInformation Tests

        [TestMethod]
        public void PathInformation_ReturnsNotNull()
        {
            FileSystem.PathInformation.ShouldNotBeNull();
        }

        #endregion

        #region GetPath Tests

        [TestMethod]
        [DynamicInstanceData(nameof(KnownFolderData))]
        public void GetPath_KnownFolder_KnownFolder_ReturnsExpectedFolderPath(KnownFolder knownFolder, string expectedPath)
        {
            var path = FileSystem.GetPath(knownFolder);
            path.ToString().ShouldBe(expectedPath);
        }

        [TestMethod]
        public void GetPath_KnownFolder_UndefinedValue_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => FileSystem.GetPath(UndefinedKnownFolder));
        }

        #endregion

        #region TryGetPath Tests

        [TestMethod]
        [DynamicInstanceData(nameof(KnownFolderData))]
        public void TryGetPath_KnownFolder_KnownFolder_ReturnsTrueAndPath(KnownFolder knownFolder, string expectedPath)
        {
            var result = FileSystem.TryGetPath(knownFolder, out var path);
            result.ShouldBeTrue();
            path.ShouldNotBeNull();
            path!.ToString().ShouldBe(expectedPath);
        }

        [TestMethod]
        public void TryGetPath_KnownFolder_UndefinedValue_ReturnsFalseAndNull()
        {
            var result = FileSystem.TryGetPath(UndefinedKnownFolder, out var path);
            result.ShouldBeFalse();
            path.ShouldBeNull();
        }

        #endregion

        #region GetFile Tests

        [TestMethod]
        public void GetFile_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => FileSystem.GetFile((string)null!));
            Should.Throw<ArgumentNullException>(() => FileSystem.GetFile((StoragePath)null!));
        }

        [TestMethod]
        public void GetFile_StoragePath_ReturnsFileWithPath()
        {
            var path = FileSystem.GetPath(Default.PathName);
            var file = FileSystem.GetFile(path);
            file.Path.ShouldBe(path);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(ValidPathStringData))]
        public void GetFile_String_ValidPathString_ReturnsFileWithPath(string pathString)
        {
            var file = FileSystem.GetFile(pathString);
            file.Path.ToString().ShouldBe(pathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(InvalidPathStringData))]
        public void GetFile_String_InvalidPathString_ThrowsArgumentException(string invalidPathString)
        {
            Should.Throw<ArgumentException>(() => FileSystem.GetFile(invalidPathString));
        }

        [TestMethod]
        public void GetFile_StoragePath_RootFolderPath_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => FileSystem.GetFile(RootFolder.Path));
        }
        
        [TestMethod]
        public void GetFile_String_RootFolderPath_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => FileSystem.GetFile(RootFolder.Path));
        }

        #endregion

        #region TryGetFile Tests

        [TestMethod]
        public void TryGetFile_StoragePath_NullParameters_ReturnsFalseAndNull()
        {
            var result = FileSystem.TryGetFile((StoragePath?)null, out var file);
            result.ShouldBeFalse();
            file.ShouldBeNull();
        }
        
        [TestMethod]
        public void TryGetFile_String_NullParameters_ReturnsFalseAndNull()
        {
            var result = FileSystem.TryGetFile((string?)null, out var file);
            result.ShouldBeFalse();
            file.ShouldBeNull();
        }

        [TestMethod]
        public void TryGetFile_StoragePath_ReturnsTrueAndFileWithPath()
        {
            var path = FileSystem.GetPath(Default.PathName);
            var result = FileSystem.TryGetFile(path, out var file);
            result.ShouldBeTrue();
            file.ShouldNotBeNull();
            file!.Path.ShouldBe(path);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(ValidPathStringData))]
        public void TryGetFile_String_ValidPathString_ReturnsTrueAndFileWithPath(string pathString)
        {
            var result = FileSystem.TryGetFile(pathString, out var file);
            result.ShouldBeTrue();
            file.ShouldNotBeNull();
            file!.Path.ToString().ShouldBe(pathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(InvalidPathStringData))]
        public void TryGetFile_String_InvalidPathString_ReturnsFalseAndNull(string invalidPathString)
        {
            var result = FileSystem.TryGetFile(invalidPathString, out var file);
            result.ShouldBeFalse();
            file.ShouldBeNull();
        }

        [TestMethod]
        public void TryGetFile_StoragePath_RootFolderPath_ReturnsFalseAndNull()
        {
            var rootPath = RootFolder.Path;
            var result = FileSystem.TryGetFile(rootPath, out var file);
            result.ShouldBeFalse();
            file.ShouldBeNull();
        }

        [TestMethod]
        public void TryGetFile_String_RootFolderPath_ReturnsFalseAndNull()
        {
            var rootPath = RootFolder.Path.ToString();
            var result = FileSystem.TryGetFile(rootPath, out var file);
            result.ShouldBeFalse();
            file.ShouldBeNull();
        }

        #endregion

        #region GetFolder Tests

        [TestMethod]
        public void GetFolder_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => FileSystem.GetFolder((string)null!));
            Should.Throw<ArgumentNullException>(() => FileSystem.GetFolder((StoragePath)null!));
        }

        [TestMethod]
        public void GetFolder_StoragePath_ReturnsFileWithPath()
        {
            var path = FileSystem.GetPath(Default.PathName);
            var folder = FileSystem.GetFolder(path);
            folder.Path.ShouldBe(path);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(ValidPathStringData))]
        public void GetFolder_String_ValidPathString_ReturnsFileWithPath(string pathString)
        {
            var folder = FileSystem.GetFolder(pathString);
            folder.Path.ToString().ShouldBe(pathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(InvalidPathStringData))]
        public void GetFolder_String_InvalidPathString_ThrowsArgumentException(string invalidPathString)
        {
            Should.Throw<ArgumentException>(() => FileSystem.GetFolder(invalidPathString));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(KnownFolderData))]
        public void GetFolder_KnownFolder_KnownFolder_ReturnsExpectedFolder(KnownFolder knownFolder, string expectedPath)
        {
            var folder = FileSystem.GetFolder(knownFolder);
            folder.Path.ToString().ShouldBe(expectedPath);
        }

        [TestMethod]
        public void GetFolder_KnownFolder_UndefinedValue_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => FileSystem.GetFolder(UndefinedKnownFolder));
        }

        #endregion

        #region TryGetFolder Tests

        [TestMethod]
        public void TryGetFolder_StoragePath_NullParameters_ReturnsFalseAndNull()
        {
            var result = FileSystem.TryGetFolder((StoragePath?)null, out var folder);
            result.ShouldBeFalse();
            folder.ShouldBeNull();
        }

        [TestMethod]
        public void TryGetFolder_String_NullParameters_ReturnsFalseAndNull()
        {
            var result = FileSystem.TryGetFolder((string?)null, out var folder);
            result.ShouldBeFalse();
            folder.ShouldBeNull();
        }

        [TestMethod]
        public void TryGetFolder_StoragePath_ReturnsTrueAndFileWithPath()
        {
            var path = FileSystem.GetPath(Default.PathName);
            var result = FileSystem.TryGetFolder(path, out var folder);
            result.ShouldBeTrue();
            folder.ShouldNotBeNull();
            folder!.Path.ShouldBe(path);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(ValidPathStringData))]
        public void TryGetFolder_String_ValidPathString_ReturnsTrueAndFileWithPath(string pathString)
        {
            var result = FileSystem.TryGetFolder(pathString, out var folder);
            result.ShouldBeTrue();
            folder.ShouldNotBeNull();
            folder!.Path.ToString().ShouldBe(pathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(InvalidPathStringData))]
        public void TryGetFolder_String_InvalidPathString_ReturnsFalseAndNull(string invalidPathString)
        {
            var result = FileSystem.TryGetFolder(invalidPathString, out var folder);
            result.ShouldBeFalse();
            folder.ShouldBeNull();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(KnownFolderData))]
        public void TryGetFolder_KnownFolder_KnownFolder_ReturnsTrueAndFolder(KnownFolder knownFolder, string expectedPath)
        {
            var result = FileSystem.TryGetFolder(knownFolder, out var folder);
            result.ShouldBeTrue();
            folder.ShouldNotBeNull();
            folder!.Path.ToString().ShouldBe(expectedPath);
        }

        [TestMethod]
        public void TryGetFolder_KnownFolder_UndefinedValue_ReturnsFalseAndNull()
        {
            var result = FileSystem.TryGetFolder(UndefinedKnownFolder, out var folder);
            result.ShouldBeFalse();
            folder.ShouldBeNull();
        }

        #endregion
    }
}
