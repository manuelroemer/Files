namespace Files.Specification.Tests.Setup
{
    using System;
    using System.Text;
    using Moq;

    /// <summary>
    ///     Defines default values for file system structures which are used by the tests.
    ///     
    ///     Because this classes requires certain values from a file system's
    ///     <see cref="FileSystem.PathInformation"/> property, it must be setup before any test cases
    ///     can be run.
    ///     
    ///     You can easily do this by calling <see cref="Setup(FileSystem)"/> before running any tests.
    ///     If you are only testing one file system in your project, you can easily do this during
    ///     the creation of your custom <see cref="FileSystemTestContext"/>.
    /// </summary>
    public static class Default
    {
        private static char? _extensionSeparator;
        private static char? _directorySeparator;

        private static char ExtensionSeparator
        {
            get => _extensionSeparator ?? throw MissingPropertyException();
            set => _extensionSeparator = value;
        }

        private static char DirectorySeparator
        {
            get => _directorySeparator ?? throw MissingPropertyException();
            set => _directorySeparator = value;
        }

        private static Exception MissingPropertyException() =>
            new InvalidOperationException(
                $"The {nameof(Default)} class has not been setup yet. " +
                $"Please ensure that the property is set to the appropriate value of your file system under test. " +
                $"You can do this with the {nameof(Default)}.{nameof(Setup)} function."
            );

        /// <summary>
        ///     Automatically sets up the <see cref="Default"/> class with values taken from the specified file system.
        /// </summary>
        public static void Setup(FileSystem fileSystem)
        {
            ExtensionSeparator = fileSystem.PathInformation.ExtensionSeparatorChar;
            DirectorySeparator = fileSystem.PathInformation.DirectorySeparatorChar;
        }

        private static string FormatExtension(string format) =>
            string.Format(format, ExtensionSeparator);

        private static string FormatSeparators(string format) =>
            string.Format(format, DirectorySeparator);



        public static CreationCollisionOption InvalidCreationCollisionOption => (CreationCollisionOption)(-1);
        public static NameCollisionOption InvalidNameCollisionOption => (NameCollisionOption)(-1);
        public static DeletionOption InvalidDeletionOption => (DeletionOption)(-1);

        public static StoragePath ForeignFileSystemPath => new Mock<StoragePath>(FileName, MockBehavior.Strict) { CallBase = true }.Object;

        public static string TextContent => "Hello World! \n\n This is the default file content used during testing.";
        public static byte[] ByteContent => Encoding.UTF8.GetBytes(TextContent);

        public static string FileName => FormatExtension("defaultFile{0}ext");
        public static string FolderName => "defaultFolder";
        public static string SharedFileFolderName => "maybeFileMaybeFolder";

        public static string ConflictingFileName => FormatExtension("conflictingFile{0}ext");
        public static string ConflictingFolderName => "conflictingFolder";

        public static string RenamedFileName => FormatExtension("renamedFile{0}ext");
        public static string RenamedFolderName => "renamedFolder";

        public static string NonExistingParentFolderName => "nonExistingParentFolder";
        public static string FileWithNonExistingParentName => FormatExtension("fileWithoutParent{0}ext");
        public static string FolderWithNonExistingParentName => "folderWithoutParent";

        public static string[] FileWithNonExistingParentSegments => new[]
        {
            NonExistingParentFolderName,
            FileWithNonExistingParentName
        };

        public static string[] FolderWithNonExistingParentSegments => new[] 
        {
            NonExistingParentFolderName,
            FolderWithNonExistingParentName
        };

        public static string SrcParentFolderName => "src";
        public static string DstParentFolderName => "dst";
        public static string SrcFileName => "srcFile.ext";
        public static string DstFileName => FormatExtension("dstFile{0}ext");
        public static string SrcFolderName => "srcFolder";
        public static string DstFolderName => "dstFolder";

        public static string[] SrcFileSegments => new[] { SrcParentFolderName, SrcFileName };
        public static string[] DstFileSegments => new[] { DstParentFolderName, DstFileName };
        public static string[] SrcFolderSegments => new[] { SrcParentFolderName, SrcFolderName };
        public static string[] DstFolderSegments => new[] { DstParentFolderName, DstFolderName };
        public static string[] SharedFileFolderInSrcSegments => new[] { SrcParentFolderName, SharedFileFolderName };
        public static string[] SharedFileFolderInDstSegments => new[] { DstParentFolderName, SharedFileFolderName };



        public static string PathName => $"{PathNameWithoutExtension}{ExtensionSeparator}{PathNameExtension}";
        public static string PathNameWithoutExtension => "name";
        public static string PathNameExtension => "ext";
    }
}
