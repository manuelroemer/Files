namespace Files.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Shouldly;

    [TestClass]
    public class StoragePathTests
    {
        private const string MockPathString = "MockPathString";

        private static readonly PathInformation PathInfoOrdinal = new PathInformation(
            invalidPathChars: Array.Empty<char>(),
            invalidFileNameChars: Array.Empty<char>(),
            directorySeparatorChar: '/',
            altDirectorySeparatorChar: '\\',
            extensionSeparatorChar: '.',
            volumeSeparatorChar: ':',
            currentDirectorySegment: ".",
            parentDirectorySegment: "..",
            defaultStringComparison: StringComparison.Ordinal
        );
        
        private static readonly PathInformation PathInfoOrdinalIgnoreCase = new PathInformation(
            invalidPathChars: Array.Empty<char>(),
            invalidFileNameChars: Array.Empty<char>(),
            directorySeparatorChar: '/',
            altDirectorySeparatorChar: '\\',
            extensionSeparatorChar: '.',
            volumeSeparatorChar: ':',
            currentDirectorySegment: ".",
            parentDirectorySegment: "..",
            defaultStringComparison: StringComparison.OrdinalIgnoreCase
        );

        private readonly Mock<FileSystem> _ordinalFsMock;
        private readonly Mock<FileSystem> _ordinalIgnoreCaseFsMock;
        private readonly Mock<StoragePath> _pathMock;
        private readonly Mock<StoragePath> _ordinalPathMock;
        private readonly Mock<StoragePath> _ordinalIgnoreCasePathMock;

        public StoragePathTests()
        {
            _ordinalFsMock = new Mock<FileSystem>() { CallBase = true };
            _ordinalFsMock.SetupGet(fs => fs.PathInformation).Returns(PathInfoOrdinal);
            
            _ordinalIgnoreCaseFsMock = new Mock<FileSystem>() { CallBase = true };
            _ordinalIgnoreCaseFsMock.SetupGet(fs => fs.PathInformation).Returns(PathInfoOrdinalIgnoreCase);

            _pathMock = new Mock<StoragePath>(MockPathString) { CallBase = true };

            _ordinalPathMock = new Mock<StoragePath>(MockPathString) { CallBase = true };
            _ordinalPathMock.Setup(path => path.FileSystem).Returns(_ordinalFsMock.Object);

            _ordinalIgnoreCasePathMock = new Mock<StoragePath>(MockPathString) { CallBase = true };
            _ordinalIgnoreCasePathMock.Setup(path => path.FileSystem).Returns(_ordinalIgnoreCaseFsMock.Object);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_NullParameters_ThrowsArgumentNullException()
        {
            Should
                .Throw<Exception>(() => new Mock<StoragePath>(args: new[] { (string?)null }) { CallBase = true }.Object)
                .InnerException.ShouldBeOfType<ArgumentNullException>();
        }
        
        [TestMethod]
        public void Constructor_EmptyString_ThrowsArgumentException()
        {
            Should
                .Throw<Exception>(() => new Mock<StoragePath>(args: new[] { "" }) { CallBase = true }.Object)
                .InnerException.ShouldBeOfType<ArgumentException>();
        }

        #endregion

        #region Length Tests

        [TestMethod]
        public void Length_StandardPath_ReturnsUnderlyingStringLength()
        {
            _pathMock.Object.Length.ShouldBe(MockPathString.Length);
        }

        #endregion

        #region Combine Tests

        [TestMethod]
        public void Combine_StoragePath_CallsCombineString()
        {
            _pathMock.Object.Combine(_pathMock.Object);
            _pathMock.Verify(path => path.Combine(MockPathString));
        }

        #endregion

        #region TryCombine Tests

        [TestMethod]
        public void TryCombine_StoragePath_CallsTryCombineString()
        {
            _pathMock.Object.TryCombine(_pathMock.Object, out var result);
            _pathMock.Verify(path => path.TryCombine(MockPathString, out result));
        }

        #endregion

        #region Join Tests

        [TestMethod]
        public void Join_StoragePath_CallsJoinString()
        {
            _pathMock.Object.Join(_pathMock.Object);
            _pathMock.Verify(path => path.Join(MockPathString));
        }

        #endregion

        #region TryJoin Tests

        [TestMethod]
        public void TryJoin_StoragePath_CallsTryJoinString()
        {
            _pathMock.Object.TryJoin(_pathMock.Object, out var result);
            _pathMock.Verify(path => path.TryJoin(MockPathString, out result));
        }

        #endregion

        #region Equals Tests

        [TestMethod]
        public void Equals_NullParameter_ReturnsFalse()
        {
            _ordinalPathMock.Object.Equals((object?)null).ShouldBeFalse();
            _ordinalPathMock.Object!.Equals((string?)null).ShouldBeFalse();
            _ordinalPathMock.Object!.Equals((StoragePath?)null).ShouldBeFalse();
        }
        
        [TestMethod]
        public void Equals_WithoutStringComparison_UsesDefaultStringComparison()
        {
            var otherPathStr = MockPathString.ToUpperInvariant();
            var otherPath = new Mock<StoragePath>(otherPathStr) { CallBase = true }.Object;

            _ordinalPathMock.Object.Equals(otherPathStr).ShouldBeFalse();
            _ordinalIgnoreCasePathMock.Object.Equals(otherPathStr).ShouldBeTrue();

            _ordinalPathMock.Object.Equals(otherPath).ShouldBeFalse();
            _ordinalIgnoreCasePathMock.Object.Equals(otherPath).ShouldBeTrue();
        }

        [TestMethod]
        public void Equals_WithStringComparison_UsesStringComparison()
        {
            var otherPathStr = MockPathString.ToUpperInvariant();
            var otherPath = new Mock<StoragePath>(otherPathStr) { CallBase = true }.Object;

            _ordinalPathMock.Object.Equals(otherPathStr, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
            _ordinalIgnoreCasePathMock.Object.Equals(otherPathStr, StringComparison.Ordinal).ShouldBeFalse();

            _ordinalPathMock.Object.Equals(otherPath, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
            _ordinalIgnoreCasePathMock.Object.Equals(otherPath, StringComparison.Ordinal).ShouldBeFalse();
        }

        #endregion

        #region GetHashCode Tests

        [TestMethod]
        public void GetHashCode_StandardPath_ReturnsUnderlyingStringsHashCode()
        {
            _pathMock.Object.GetHashCode().ShouldBe(MockPathString.GetHashCode());
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_StandardPath_ReturnsUnderlyingString()
        {
            _pathMock.Object.ToString().ShouldBe(MockPathString);
        }

        #endregion

        #region operator+ Tests

        [TestMethod]
        public void OpAddition_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => (StoragePath)null! + "");
            Should.Throw<ArgumentNullException>(() => _pathMock.Object + null!);
        }

        [TestMethod]
        public void OpAddition_ValidParameters_CallsAppend()
        {
            _ = _pathMock.Object + "";
            _pathMock.Verify(path => path.Append(""));
        }

        #endregion

        #region operator/ Tests

        [TestMethod]
        public void OpDivision_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => (StoragePath)null! / "");
            Should.Throw<ArgumentNullException>(() => _pathMock.Object / (string)null!);
            Should.Throw<ArgumentNullException>(() => _pathMock.Object / (StoragePath)null!);
        }

        [TestMethod]
        public void OpDivision_String_ValidParameters_CallsJoin()
        {
            _ = _pathMock.Object / MockPathString;
            _pathMock.Verify(path => path.Join(MockPathString));
        }
        
        [TestMethod]
        public void OpDivision_StoragePath_ValidParameters_CallsJoin()
        {
            _ = _pathMock.Object / _pathMock.Object;
            _pathMock.Verify(path => path.Join(MockPathString));
        }

        #endregion

        #region operator==

        [TestMethod]
        public void OpEquality_UsesDefaultStringComparison()
        {
            var otherPathStr = MockPathString.ToUpperInvariant();
            var otherPath = new Mock<StoragePath>(otherPathStr) { CallBase = true }.Object;

            (_ordinalPathMock.Object == otherPathStr).ShouldBeFalse();
            (_ordinalIgnoreCasePathMock.Object == otherPathStr).ShouldBeTrue();

            (_ordinalPathMock.Object ==  otherPath).ShouldBeFalse();
            (_ordinalIgnoreCasePathMock.Object == otherPath).ShouldBeTrue();

            (otherPathStr == _ordinalPathMock.Object).ShouldBeFalse();
            (otherPathStr == _ordinalIgnoreCasePathMock.Object).ShouldBeTrue();
        }

        #endregion

        #region OpInequality

        [TestMethod]
        public void OpInequality_UsesDefaultStringComparison()
        {
            var otherPathStr = MockPathString.ToUpperInvariant();
            var otherPath = new Mock<StoragePath>(otherPathStr) { CallBase = true }.Object;

            (_ordinalPathMock.Object != otherPathStr).ShouldBeTrue();
            (_ordinalIgnoreCasePathMock.Object != otherPathStr).ShouldBeFalse();

            (_ordinalPathMock.Object != otherPath).ShouldBeTrue();
            (_ordinalIgnoreCasePathMock.Object != otherPath).ShouldBeFalse();

            (otherPathStr != _ordinalPathMock.Object).ShouldBeTrue();
            (otherPathStr != _ordinalIgnoreCasePathMock.Object).ShouldBeFalse();
        }

        #endregion

        #region implicit operator string

        [TestMethod]
        public void OpImplicitString_StandardPath_ReturnsUnderlyingString()
        {
            string? pathStr = _pathMock.Object;
            pathStr.ShouldBe(MockPathString);
        }

        [TestMethod]
        public void OpImplicitString_NullPath_ReturnsNull()
        {
            string? pathStr = (StoragePath?)null;
            pathStr.ShouldBeNull();
        }

        #endregion
    }
}
