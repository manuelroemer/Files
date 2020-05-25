namespace Files.FileSystems.InMemory.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class InMemoryStorageFolderTests : StorageFolderSpecificationTests
    {
        public InMemoryStorageFolderTests()
            : base(InMemoryFileSystemTestContext.Instance) { }
    }
}
