namespace Files.FileSystems.WindowsStorage.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class WindowsStorageStorageFileTests : StorageFileSpecificationTests
    {
        public WindowsStorageStorageFileTests()
            : base(WindowsStorageFileSystemTestContext.Instance) { }
    }
}
