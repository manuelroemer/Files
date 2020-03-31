namespace Files.FileSystems.WindowsStorage.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalStoragePathTests : StoragePathSpecificationTests
    {
        public override IEnumerable<object[]> AppendPathsWithInvalidPartsData => InvalidPathsData;

        public override IEnumerable<object[]> CombinePathsWithInvalidOthersData => InvalidPathsData;

        public override IEnumerable<object[]> JoinPathsWithInvalidOthersData => InvalidPathsData;

        private static IEnumerable<object[]> InvalidPathsData => Path
            .GetInvalidPathChars()
            .Select(invalidChar => new[] { Default.PathName, Default.PathName + invalidChar });

        public PhysicalStoragePathTests()
            : base(WindowsStorageFileSystemTestContext.Instance) { }
    }
}
