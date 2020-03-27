namespace Files.FileSystems.Physical.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalStorageFolderTests : StorageFolderSpecificationTests
    {
        public PhysicalStorageFolderTests() : base(PhysicalFileSystemTestContext.Instance) { }
    }
}
