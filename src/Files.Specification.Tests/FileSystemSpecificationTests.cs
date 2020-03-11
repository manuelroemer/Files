namespace Files.Specification.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public abstract class FileSystemSpecificationTests : FileSystemTestBase
    {

        /// <summary>
        ///     Gets a <see cref="KnownFolder"/> value which is not supported by the file system
        ///     implementation.
        ///     By default this returns <see cref="int.MinValue"/>, casted to a <see cref="KnownFolder"/> value.
        /// </summary>
        protected virtual KnownFolder UnknownFolder { get; } = (KnownFolder)int.MinValue;

        public FileSystemSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region PathInformation Tests

        [TestMethod]
        public void PathInformation_Returns_Non_Null_Instance()
        {
            FileSystem.PathInformation.ShouldNotBeNull();
        }

        #endregion

        #region GetPath Tests

        [TestMethod]
        public void GetPath_KnownFolder_Throws_NotSupportedException_For_Unknown_Folder()
        {
            Should.Throw<NotSupportedException>(() => FileSystem.GetPath(UnknownFolder));
        }

        #endregion

        #region TryGetPath Tests

        [TestMethod]
        public void TryGetPath_KnownFolder_Fails_For_Unknown_Folder()
        {
            FileSystem.TryGetPath(UnknownFolder, out var path).ShouldBeFalse();
            path.ShouldBeNull();
        }

        #endregion

        #region GetFolder Tests

        [TestMethod]
        public void GetFolder_KnownFolder_Throws_NotSupportedException_For_Unknown_Folder()
        {
            Should.Throw<NotSupportedException>(() => FileSystem.GetFolder(UnknownFolder));
        }

        #endregion

        #region TryGetFolder Tests

        [TestMethod]
        public void TryGetFolder_KnownFolder_Fails_For_Unknown_Folder()
        {
            FileSystem.TryGetFolder(UnknownFolder, out var folder).ShouldBeFalse();
            folder.ShouldBeNull();
        }

        #endregion

    }

}
