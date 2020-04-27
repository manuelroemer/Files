namespace Files.FileSystems.Physical.Tests
{
    using System.Collections.Generic;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalStoragePathTests : StoragePathSpecificationTests
    {
        // Note on the test data:
        //
        // Ever since .NET Core 2.1, the BCL doesn't manually validate paths anymore.
        // Therefore, instead of ArgumentExceptions in the Path class, the BCL throws IOExceptions
        // when using an invalid path for file I/O.
        // Since manual path handling is incredibly error prone, we are not doing it in this library.
        // Instead, there's going to be a difference depending on the runtime. PhysicalStoragePath
        // will throw ArgumentExceptions on .NET FW/.NET Core <= 2.0, while any newer version will
        // throw delayed IOExceptions for invalid paths.
        // The specification is tuned to respect this.
        //
        // Since we must still provide data for invalid paths, e.g. for Append tests, we can abuse
        // the fact that the '\0' character continues to be illegal.
        // We can easily use it to enforce ArgumentExceptions in the PhysicalStoragePath constructor.

        public override IEnumerable<object[]> AppendPathsWithInvalidPartsData => new[]
        {
            new[] { Default.PathName, '\0'.ToString() },
        };

        public override IEnumerable<object[]> CombinePathsWithInvalidOthersData => new[]
        {
            new[] { Default.PathName, '\0'.ToString() },
        };

        public override IEnumerable<object[]> JoinPathsWithInvalidOthersData => new[]
        {
            new[] { Default.PathName, '\0'.ToString() },
        };

        public override IEnumerable<object[]> LinkPathsWithInvalidOthersData => new[]
        {
            new[] { Default.PathName, '\0'.ToString() },
        };

        public PhysicalStoragePathTests()
            : base(PhysicalFileSystemTestContext.Instance) { }
    }
}
