namespace Files.FileSystems.Physical.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static System.Environment;
    using static System.Environment.SpecialFolder;

    [TestClass]
    public sealed class PhysicalFileSystemTests : FileSystemSpecificationTests
    {
        public override IEnumerable<object[]> KnownFolderData => new[]
        {
            new object[] { KnownFolder.TemporaryData, Path.GetTempPath() },
            new object[] { KnownFolder.RoamingApplicationData, GetFolderPath(ApplicationData, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.LocalApplicationData, GetFolderPath(LocalApplicationData, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.ProgramData, GetFolderPath(CommonApplicationData, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.UserProfile, GetFolderPath(UserProfile, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.Desktop, GetFolderPath(Desktop, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.DocumentsLibrary, GetFolderPath(MyDocuments, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.PicturesLibrary, GetFolderPath(MyPictures, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.VideosLibrary, GetFolderPath(MyVideos, SpecialFolderOption.DoNotVerify) },
            new object[] { KnownFolder.MusicLibrary, GetFolderPath(MyMusic, SpecialFolderOption.DoNotVerify) },
        };

        public override IEnumerable<object[]> ValidPathStringData => new[]
        {
            new object[] { Default.PathName },
            new object[] { PathInformation.CurrentDirectorySegment },
            new object[] { PathInformation.ParentDirectorySegment },
            new object[] { Path.GetTempPath() }, // Any absolute path is fine.
        };
        
        public override IEnumerable<object[]> InvalidPathStringData => new[]
        {
            // See comment in PhysicalStoragePathTests for details on why '\0' is used specifically.
            new object[] { "" },
            new object[] { "\0" },
            new object[] { Default.PathName + "\0" },
        };

        public PhysicalFileSystemTests()
            : base(PhysicalFileSystemTestContext.Instance) { }
    }
}
