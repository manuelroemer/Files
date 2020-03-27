namespace Files.Specification.Tests.Setup
{
    using System.Text;

    /// <summary>
    ///     Defines default values for file system structures which are used by the tests.
    /// </summary>
    public static class Default
    {

        public static string TextContent => "Hello World! \n\n This is the default file content used during testing.";
        public static byte[] ByteContent => Encoding.UTF8.GetBytes(TextContent);

        public static string FileName => "defaultFile.ext";
        public static string FolderName => "defaultFolder";
        public static string SharedFileFolderName => "maybeFileMaybeFolder";

        public static string ConflictingFileName => "conflictingFile.ext";
        public static string ConflictingFolderName => "conflictingFolder";

        public static string RenamedFileName => "renamedFile.ext";
        public static string RenamedFolderName => "renamedFolder";

        public static string NonExistingParentFolderName => "nonExistingParentFolder";
        public static string FileWithNonExistingParentName => "fileWithoutParent.ext";
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
        public static string DstFileName => "dstFile.ext";
        public static string SrcFolderName => "srcFolder";
        public static string DstFolderName => "dstFolder";

        public static string[] SrcFileSegments => new[] { SrcParentFolderName, SrcFileName };
        public static string[] DstFileSegments => new[] { DstParentFolderName, DstFileName };
        public static string[] SrcFolderSegments => new[] { SrcParentFolderName, SrcFolderName };
        public static string[] DstFolderSegments => new[] { DstParentFolderName, DstFolderName };
        public static string[] SharedFileFolderInSrcSegments => new[] { SrcParentFolderName, SharedFileFolderName };
        public static string[] SharedFileFolderInDstSegments => new[] { DstParentFolderName, SharedFileFolderName };

    }

}
