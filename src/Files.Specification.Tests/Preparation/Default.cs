namespace Files.Specification.Tests.Preparation
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

        public static string ConflictingFileName => "conflictingFile.ext";
        public static string ConflictingFolderName => "conflictingFolder";

        public static string SrcFolderName => "src";
        public static string DstFolderName => "dst";
        public static string SrcFileName => "srcFile.ext";
        public static string DstFileName => "dstFile.ext";

    }

}
