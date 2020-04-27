namespace Files.FileSystems.Physical.Tests
{
    using System.Collections.Generic;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalStorageFolderTests : StorageFolderSpecificationTests
    {
        public override IEnumerable<object[]> InvalidPathsData => new[]
        {
            new[] { "\0" },
        };

        public PhysicalStorageFolderTests()
            : base(PhysicalFileSystemTestContext.Instance) { }
    }
}
