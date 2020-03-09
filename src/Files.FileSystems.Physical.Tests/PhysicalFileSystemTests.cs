namespace Files.FileSystems.Physical.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalFileSystemTests : FileSystemSpecificationTests
    {

        public PhysicalFileSystemTests() : base(PhysicalFileSystemTestContext.Instance) { }

    }

}
