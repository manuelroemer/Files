namespace Files.FileSystems.InMemory.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class InMemoryStorageFileTests : StorageFileSpecificationTests
    {
        public InMemoryStorageFileTests()
            : base(InMemoryFileSystemTestContext.Instance) { }
    }
}
