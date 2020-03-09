﻿namespace Files.FileSystems.Physical.Tests
{
    using Files.Specification.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class PhysicalFileTests : FileSpecificationTests
    {

        public PhysicalFileTests() : base(PhysicalFileSystemTestContext.Instance) { }

    }

}
