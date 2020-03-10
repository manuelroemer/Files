namespace Files.FileSystems.Physical.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalStorageFileTests : StorageFileSpecificationTests
    {

        public PhysicalStorageFileTests() : base(PhysicalFileSystemTestContext.Instance) { }

    }

}
