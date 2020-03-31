namespace Files.FileSystems.WindowsStorage.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Windows.Storage;
    using static System.Environment;

    [TestClass]
    public sealed class WindowsStorageFileSystemTests : FileSystemSpecificationTests
    {
        public override IEnumerable<object[]> KnownFolderData => new[]
        {
            new object[] { KnownFolder.TemporaryData, ApplicationData.Current.TemporaryFolder.Path },
            new object[] { KnownFolder.RoamingApplicationData, ApplicationData.Current.RoamingFolder.Path },
            new object[] { KnownFolder.LocalApplicationData, ApplicationData.Current.LocalFolder.Path },
            new object[] { KnownFolder.ProgramData, GetFolderPath(SpecialFolder.CommonApplicationData) },
            new object[] { KnownFolder.UserProfile, GetFolderPath(SpecialFolder.UserProfile) },
            new object[] { KnownFolder.Desktop, GetFolderPath(SpecialFolder.Desktop) },
            new object[] { KnownFolder.DocumentsLibrary, GetFolderPath(SpecialFolder.MyDocuments) },
            new object[] { KnownFolder.PicturesLibrary, GetFolderPath(SpecialFolder.MyPictures) },
            new object[] { KnownFolder.VideosLibrary, GetFolderPath(SpecialFolder.MyVideos) },
            new object[] { KnownFolder.MusicLibrary, GetFolderPath(SpecialFolder.MyMusic) },
        };

        public override IEnumerable<object[]> ValidPathStringData => new[]
        {
            new object[] { Default.PathName },
            new object[] { PathInformation.CurrentDirectorySegment },
            new object[] { PathInformation.ParentDirectorySegment },
            new object[] { ApplicationData.Current.TemporaryFolder.Path }, // Any absolute path is fine.
        };

        public override IEnumerable<object[]> InvalidPathStringData => Path
            .GetInvalidPathChars()
            .Select(invalidChar => new[] { Default.PathName + invalidChar });

        public WindowsStorageFileSystemTests()
            : base(WindowsStorageFileSystemTestContext.Instance) { }
    }
}
