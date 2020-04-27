namespace Files.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Shouldly;
    using static Files.Tests.Mocks.StoragePathMocks;

    [TestClass]
    public class StoragePathTests
    {
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
            CreateOrdinalPathMock().Object.Length.ShouldBe(MockedPathString.Length);
        }

        #endregion

        #region Combine Tests

        [TestMethod]
        public void Combine_StoragePath_CallsCombineString()
        {
            var mock = CreateOrdinalPathMock();
            mock.Object.Combine(CreateOrdinalPathMock().Object);
            mock.Verify(path => path.Combine(MockedPathString));
        }

        #endregion

        #region TryCombine Tests

        [TestMethod]
        public void TryCombine_StoragePath_CallsTryCombineString()
        {
            var mock = CreateOrdinalPathMock();
            _ = mock.Object.TryCombine(mock.Object, out var result);
            mock.Verify(path => path.TryCombine(MockedPathString, out result));
        }

        #endregion

        #region Join Tests

        [TestMethod]
        public void Join_StoragePath_CallsJoinString()
        {
            var mock = CreateOrdinalPathMock();
            mock.Object.Join(CreateOrdinalPathMock().Object);
            mock.Verify(path => path.Join(MockedPathString));
        }

        #endregion

        #region TryJoin Tests

        [TestMethod]
        public void TryJoin_StoragePath_CallsTryJoinString()
        {
            var mock = CreateOrdinalPathMock();
            _ = mock.Object.TryJoin(mock.Object, out var result);
            mock.Verify(path => path.TryJoin(MockedPathString, out result));
        }

        #endregion

        #region CompareTo Tests

        [TestMethod]
        public void CompareTo_NullParameter_ReturnsGreater()
        {
            ((IComparable)CreateOrdinalPathMock().Object).CompareTo((object?)null).ShouldBeGreaterThan(0);
            CreateOrdinalPathMock().Object.CompareTo((string?)null).ShouldBeGreaterThan(0);
            CreateOrdinalPathMock().Object.CompareTo((StoragePath?)null).ShouldBeGreaterThan(0);
        }

        [TestMethod]
        [DataRow(MockedPathString)]
        [DataRow(UpperMockedPathString)]
        [DataRow(LowerMockedPathString)]
        public void CompareTo_StandardPath_ReturnsStringCompareResult(string otherPathStr)
        {
            var otherPath = new Mock<StoragePath>(otherPathStr) { CallBase = true }.Object;
            var expected = string.Compare(MockedPathString, otherPathStr, StringComparison.Ordinal);

            ((IComparable)CreateOrdinalPathMock().Object).CompareTo(otherPathStr).ShouldBe(expected);
            ((IComparable)CreateOrdinalPathMock().Object).CompareTo(otherPath).ShouldBe(expected);
            CreateOrdinalPathMock().Object.CompareTo(otherPathStr).ShouldBe(expected);
            CreateOrdinalPathMock().Object.CompareTo(otherPath).ShouldBe(expected);
        }

        [TestMethod]
        public void CompareTo_WithoutStringComparison_UsesDefaultStringComparison()
        {
            var upperPath = CreateOrdinalUpperPathMock().Object;

            CreateOrdinalPathMock().Object.CompareTo(upperPath).ShouldNotBe(0);
            CreateOrdinalIgnoreCasePathMock().Object.CompareTo(UpperMockedPathString).ShouldBe(0);

            CreateOrdinalPathMock().Object.CompareTo(upperPath).ShouldNotBe(0);
            CreateOrdinalIgnoreCasePathMock().Object.CompareTo(upperPath).ShouldBe(0);
        }

        [TestMethod]
        public void CompareTo_WithStringComparison_UsesStringComparison()
        {
            var upperPath = CreateOrdinalUpperPathMock().Object;

            CreateOrdinalPathMock().Object.CompareTo(UpperMockedPathString, StringComparison.OrdinalIgnoreCase).ShouldBe(0);
            CreateOrdinalIgnoreCasePathMock().Object.CompareTo(UpperMockedPathString, StringComparison.Ordinal).ShouldNotBe(0);

            CreateOrdinalPathMock().Object.CompareTo(upperPath, StringComparison.OrdinalIgnoreCase).ShouldBe(0);
            CreateOrdinalIgnoreCasePathMock().Object.CompareTo(upperPath, StringComparison.Ordinal).ShouldNotBe(0);
        }

        #endregion

        #region Equals Tests

        [TestMethod]
        public void Equals_NullParameter_ReturnsFalse()
        {
            CreateOrdinalPathMock().Object.Equals((object?)null).ShouldBeFalse();
            CreateOrdinalPathMock().Object!.Equals((string?)null).ShouldBeFalse();
            CreateOrdinalPathMock().Object!.Equals((StoragePath?)null).ShouldBeFalse();
        }
        
        [TestMethod]
        public void Equals_WithoutStringComparison_UsesDefaultStringComparison()
        {
            var upperPath = CreateOrdinalUpperPathMock().Object;

            CreateOrdinalPathMock().Object.Equals(UpperMockedPathString).ShouldBeFalse();
            CreateOrdinalIgnoreCasePathMock().Object.Equals(UpperMockedPathString).ShouldBeTrue();

            CreateOrdinalPathMock().Object.Equals(upperPath).ShouldBeFalse();
            CreateOrdinalIgnoreCasePathMock().Object.Equals(upperPath).ShouldBeTrue();
        }

        [TestMethod]
        public void Equals_WithStringComparison_UsesStringComparison()
        {
            var upperPath = CreateOrdinalUpperPathMock().Object;

            CreateOrdinalPathMock().Object.Equals(UpperMockedPathString, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
            CreateOrdinalIgnoreCasePathMock().Object.Equals(UpperMockedPathString, StringComparison.Ordinal).ShouldBeFalse();

            CreateOrdinalPathMock().Object.Equals(upperPath, StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
            CreateOrdinalIgnoreCasePathMock().Object.Equals(upperPath, StringComparison.Ordinal).ShouldBeFalse();
        }

        #endregion

        #region GetHashCode Tests

        [TestMethod]
        public void GetHashCode_StandardPath_ReturnsUnderlyingStringsHashCode()
        {
            CreateOrdinalPathMock().Object.GetHashCode().ShouldBe(MockedPathString.GetHashCode());
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_StandardPath_ReturnsUnderlyingString()
        {
            CreateOrdinalPathMock().Object.ToString().ShouldBe(MockedPathString);
        }

        #endregion

        #region operator+ Tests

        [TestMethod]
        public void OpAddition_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => (StoragePath)null! + "");
            Should.Throw<ArgumentNullException>(() => CreateOrdinalPathMock().Object + null!);
        }

        [TestMethod]
        public void OpAddition_ValidParameters_CallsAppend()
        {
            var mock = CreateOrdinalPathMock();
            _ = mock.Object + "";
            mock.Verify(path => path.Append(""));
        }

        #endregion

        #region operator/ Tests

        [TestMethod]
        public void OpDivision_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => (StoragePath)null! / "");
            Should.Throw<ArgumentNullException>(() => CreateOrdinalPathMock().Object / (string)null!);
            Should.Throw<ArgumentNullException>(() => CreateOrdinalPathMock().Object / (StoragePath)null!);
        }

        [TestMethod]
        public void OpDivision_String_ValidParameters_CallsJoin()
        {
            var mock = CreateOrdinalPathMock();
            _ = mock.Object / MockedPathString;
            mock.Verify(path => path.Join(MockedPathString));
        }
        
        [TestMethod]
        public void OpDivision_StoragePath_ValidParameters_CallsJoin()
        {
            var mock = CreateOrdinalPathMock();
            _ = mock.Object / mock.Object;
            mock.Verify(path => path.Join(MockedPathString));
        }

        #endregion

        #region operator==

        [TestMethod]
        public void OpEquality_UsesDefaultStringComparison()
        {
            var otherPathStr = UpperMockedPathString;
            var otherPath = CreateOrdinalUpperPathMock().Object;

            (CreateOrdinalPathMock().Object == otherPathStr).ShouldBeFalse();
            (CreateOrdinalIgnoreCasePathMock().Object == otherPathStr).ShouldBeTrue();

            (CreateOrdinalPathMock().Object ==  otherPath).ShouldBeFalse();
            (CreateOrdinalIgnoreCasePathMock().Object == otherPath).ShouldBeTrue();

            (otherPathStr == CreateOrdinalPathMock().Object).ShouldBeFalse();
            (otherPathStr == CreateOrdinalIgnoreCasePathMock().Object).ShouldBeTrue();
        }

        #endregion

        #region operator!=

        [TestMethod]
        public void OpInequality_UsesDefaultStringComparison()
        {
            var otherPathStr = UpperMockedPathString;
            var otherPath = CreateOrdinalUpperPathMock().Object;

            (CreateOrdinalPathMock().Object != otherPathStr).ShouldBeTrue();
            (CreateOrdinalIgnoreCasePathMock().Object != otherPathStr).ShouldBeFalse();

            (CreateOrdinalPathMock().Object != otherPath).ShouldBeTrue();
            (CreateOrdinalIgnoreCasePathMock().Object != otherPath).ShouldBeFalse();

            (otherPathStr != CreateOrdinalPathMock().Object).ShouldBeTrue();
            (otherPathStr != CreateOrdinalIgnoreCasePathMock().Object).ShouldBeFalse();
        }

        #endregion

        #region implicit operator string

        [TestMethod]
        public void OpImplicitString_StandardPath_ReturnsUnderlyingString()
        {
            string? pathStr = CreateOrdinalPathMock().Object;
            pathStr.ShouldBe(MockedPathString);
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
