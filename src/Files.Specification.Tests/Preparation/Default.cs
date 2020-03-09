namespace Files.Specification.Tests.Preparation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Defines default values for file system structures which are used by the tests.
    /// </summary>
    public static class Default
    {

        public static string TextContent => "Hello World! \n\n This is the default file content used during testing.";
        public static byte[] ByteContent => Encoding.UTF8.GetBytes(TextContent);

        public static string FileName => "default.file";
        public static string FolderName => "defaultFolder";

        public static string ConflictingFileName => "conflicting.file";
        public static string ConflictingFolderName => "conflictingFolder";

        public static string SrcFolderName => "src";
        public static string DstFolderName => "dst";
        public static string SrcFileName => "src.file";
        public static string DstFileName => "dst.file";

    }

}
