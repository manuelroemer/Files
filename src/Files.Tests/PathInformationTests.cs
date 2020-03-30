namespace Files.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;
    using System.Collections.ObjectModel;

    [TestClass]
    public class PathInformationTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_StandardParameters_SetsProperties()
        {
            var invalidPathChars = new[] { 'a', 'b', 'c' };
            var invalidFileNameChars = new[] { 'd', 'e', 'f' };
            var directorySeparator = '/';
            var altDirectorySeparator = '/';
            var extensionSeparator = '.';
            var volumeSeparator = ':';
            var currentDirectorySegment = ".";
            var parentDirectorySegment = "..";
            var defaultStringComparison = StringComparison.OrdinalIgnoreCase;

            var info = new PathInformation(
                invalidPathChars,
                invalidFileNameChars,
                directorySeparator,
                altDirectorySeparator,
                extensionSeparator,
                volumeSeparator,
                currentDirectorySegment,
                parentDirectorySegment,
                defaultStringComparison
            );

            info.InvalidPathChars.ShouldBe(invalidPathChars);
            info.InvalidFileNameChars.ShouldBe(invalidFileNameChars);
            info.DirectorySeparatorChar.ShouldBe(directorySeparator);
            info.AltDirectorySeparatorChar.ShouldBe(altDirectorySeparator);
            info.ExtensionSeparatorChar.ShouldBe(extensionSeparator);
            info.VolumeSeparatorChar.ShouldBe(volumeSeparator);
            info.CurrentDirectorySegment.ShouldBe(currentDirectorySegment);
            info.ParentDirectorySegment.ShouldBe(parentDirectorySegment);
            info.DefaultStringComparison.ShouldBe(defaultStringComparison);

            info.InvalidPathChars.ShouldBeOfType<ReadOnlyCollection<char>>();
            info.InvalidFileNameChars.ShouldBeOfType<ReadOnlyCollection<char>>();
            info.DirectorySeparatorChars.ShouldBeOfType<ReadOnlyCollection<char>>();
            info.DirectorySeparatorChars.ShouldBe(new[] { '/' });
        }

        #endregion
    }
}
