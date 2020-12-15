namespace Files.FileSystems.InMemory.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class InMemoryFileSystemTests : FileSystemSpecificationTests
    {
        public override IEnumerable<object[]> KnownFolderData =>
            Enum.GetValues(typeof(KnownFolder))
            .Cast<KnownFolder>()
            .Select(knownFolder => new object[]
            {
                knownFolder,
                DefaultKnownFolderProvider.Default.GetPath(
                    (InMemoryFileSystem)InMemoryFileSystemTestContext.Instance.FileSystem,
                    knownFolder
                )
                .ToString()
            });

        public override IEnumerable<object[]> ValidPathStringData => new[]
        {
            new object[] { Default.PathName },
        };

        public override IEnumerable<object[]> InvalidPathStringData => new[]
        {
            new object[] { "" },
            new object[] { "\0" },
            new object[] { Default.PathName + "\0" },
        };

        public InMemoryFileSystemTests()
            : base(InMemoryFileSystemTestContext.Instance) { }
    }
}
