namespace Files.FileSystems.Physical.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Files.Utilities;
    using IOPath = System.IO.Path;

    internal static class PathHelper
    {

        internal static readonly char[] InvalidPathChars = IOPath.GetInvalidPathChars();
        internal static readonly char[] InvalidFileNameChars = IOPath.GetInvalidFileNameChars();
        internal static readonly char[] InvalidPathOrFileNameChars = InvalidPathChars.Concat(InvalidFileNameChars).Distinct().ToArray();
        internal static readonly char[] PathSeparatorChars = 
            IOPath.DirectorySeparatorChar == IOPath.AltDirectorySeparatorChar
                ? new[] { IOPath.DirectorySeparatorChar }
                : new[] { IOPath.DirectorySeparatorChar, IOPath.AltDirectorySeparatorChar };

        public static StringComparison OSStringComparison { get; } =
            Environment.OSVersion.Platform == PlatformID.Win32NT
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        public static string? GetExtensionWithoutTrailingExtensionSeparator(string? path)
        {
            return TrimTrailingExtensionSeparator(IOPath.GetExtension(path));
        }

        [return: NotNullIfNotNull("extension")]
        public static string? TrimTrailingExtensionSeparator(string? extension)
        {
            if (string.IsNullOrEmpty(extension) || !extension.StartsWith('.'))
            {
                return extension;
            }
            return extension.Substring(1);
        }

        public static string[] GetSegments(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            return path.Split(PathSeparatorChars);
        }

        public static bool ContainsInvalidPathChars(string path) =>
            path.Contains(InvalidPathChars);

        public static bool ContainsInvalidFileNameChars(string path) =>
            path.Contains(InvalidFileNameChars);

        public static bool ContainsInvalidPathOrFileNameChars(string path) =>
            path.Contains(InvalidPathOrFileNameChars);

        public static bool IsFileOrDirectoryNameOnly(string path) =>
            !path.Contains(PathSeparatorChars);

    }

}
