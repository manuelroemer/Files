namespace Files.Tests
{
    using System;
    using System.Collections.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public class PathInformationTests
    {
        private static readonly char[] InvalidPathChars = new[] { 'a', 'b', 'c' };
        private static readonly char[] InvalidFileNameChars = new[] { 'd', 'e', 'f' };
        private const char DirectorySeparator = '/';
        private const char AltDirectorySeparator = '/';
        private const char ExtensionSeparator = '.';
        private const char VolumeSeparator = ':';
        private const string CurrentDirectorySegment = ".";
        private const string ParentDirectorySegment = "..";
        private const StringComparison DefaultStringComparison = StringComparison.OrdinalIgnoreCase;

        #region Constructor Tests

        [TestMethod]
        public void Constructor_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => new PathInformation(
                null!,
                InvalidFileNameChars,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                CurrentDirectorySegment,
                ParentDirectorySegment,
                DefaultStringComparison
            ));

            Should.Throw<ArgumentNullException>(() => new PathInformation(
                InvalidPathChars,
                null!,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                CurrentDirectorySegment,
                ParentDirectorySegment,
                DefaultStringComparison
            ));

            Should.Throw<ArgumentNullException>(() => new PathInformation(
                InvalidFileNameChars,
                InvalidFileNameChars,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                null!,
                ParentDirectorySegment,
                DefaultStringComparison
            ));

            Should.Throw<ArgumentNullException>(() => new PathInformation(
                InvalidFileNameChars,
                InvalidFileNameChars,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                CurrentDirectorySegment,
                null!,
                DefaultStringComparison
            ));
        }

        [TestMethod]
        public void Constructor_EmptyStrings_ThrowsArgumentException()
        {
            Should.Throw<ArgumentException>(() => new PathInformation(
                InvalidPathChars,
                InvalidFileNameChars,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                "",
                ParentDirectorySegment,
                DefaultStringComparison
            ));

            Should.Throw<ArgumentException>(() => new PathInformation(
                InvalidPathChars,
                InvalidFileNameChars,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                CurrentDirectorySegment,
                "",
                DefaultStringComparison
            ));
        }

        [TestMethod]
        public void Constructor_StandardParameters_SetsProperties()
        {
            var info = new PathInformation(
                InvalidPathChars,
                InvalidFileNameChars,
                DirectorySeparator,
                AltDirectorySeparator,
                ExtensionSeparator,
                VolumeSeparator,
                CurrentDirectorySegment,
                ParentDirectorySegment,
                DefaultStringComparison
            );

            info.InvalidPathChars.ShouldBe(InvalidPathChars);
            info.InvalidFileNameChars.ShouldBe(InvalidFileNameChars);
            info.DirectorySeparatorChar.ShouldBe(DirectorySeparator);
            info.AltDirectorySeparatorChar.ShouldBe(AltDirectorySeparator);
            info.ExtensionSeparatorChar.ShouldBe(ExtensionSeparator);
            info.VolumeSeparatorChar.ShouldBe(VolumeSeparator);
            info.CurrentDirectorySegment.ShouldBe(CurrentDirectorySegment);
            info.ParentDirectorySegment.ShouldBe(ParentDirectorySegment);
            info.DefaultStringComparison.ShouldBe(DefaultStringComparison);

            info.InvalidPathChars.ShouldBeOfType<ReadOnlyCollection<char>>();
            info.InvalidFileNameChars.ShouldBeOfType<ReadOnlyCollection<char>>();
            info.DirectorySeparatorChars.ShouldBeOfType<ReadOnlyCollection<char>>();
            info.DirectorySeparatorChars.ShouldBe(new[] { '/' });
        }

        #endregion
    }
}
