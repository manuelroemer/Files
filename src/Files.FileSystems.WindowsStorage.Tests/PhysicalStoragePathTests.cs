namespace Files.FileSystems.WindowsStorage.Tests
{
    using System.Collections.Generic;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalStoragePathTests : StoragePathSpecificationTests
    {
        public override IEnumerable<object[]> AppendPathsWithInvalidPartsData => new[]
        {
            new[] { Default.PathName, "\0" },
        };

        public override IEnumerable<object[]> CombinePathsWithInvalidOthersData => new[]
        {
            new[] { Default.PathName, "\0" },
        };

        public override IEnumerable<object[]> JoinPathsWithInvalidOthersData => new[]
        {
            new[] { Default.PathName, "\0" },
        };

        public override IEnumerable<object[]> LinkPathsWithInvalidOthersData => new[]
        {
            new[] { Default.PathName, "\0" },
        };

        public PhysicalStoragePathTests()
            : base(WindowsStorageFileSystemTestContext.Instance) { }
    }
}
