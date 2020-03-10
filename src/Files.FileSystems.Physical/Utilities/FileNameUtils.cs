namespace Files.FileSystems.Physical.Utilities
{
    using System.IO;

    internal static class FileNameUtils
    {

        public static string? GetRealName(this FileInfo fileInfo)
        {
            var realChildren = fileInfo.Directory.GetFiles(searchPattern: fileInfo.Name);
            if (realChildren.Length != 1)
            {
                // There is either no child with this name or too many to make a pick.
                return null;
            }
            else
            {
                return realChildren[0].Name;
            }
        }
        
        public static string? GetRealName(this DirectoryInfo directoryInfo)
        {
            var realChildren = directoryInfo.Parent?.GetFiles(searchPattern: directoryInfo.Name);
            if (realChildren is null || realChildren.Length != 1)
            {
                // There is either no child with this name or too many to make a pick.
                return null;
            }
            else
            {
                return realChildren[0].Name;
            }
        }

    }

}
