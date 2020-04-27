namespace Files.FileSystems.WindowsStorage.Tests
{
    using System.Collections.Generic;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class WindowsStorageStorageFileTests : StorageFileSpecificationTests
    {
        public override IEnumerable<object[]> InvalidPathsData => new[]
        {
            new[] { "\0" },
        };

        public WindowsStorageStorageFileTests()
            : base(WindowsStorageFileSystemTestContext.Instance) { }
    }
}
