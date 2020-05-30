namespace Files.FileSystems.WindowsStorage.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class WindowsStorageStorageFolderTests : StorageFolderSpecificationTests
    {
        public WindowsStorageStorageFolderTests()
            : base(WindowsStorageFileSystemTestContext.Instance) { }
    }
}
