namespace Files.Shared.PhysicalStoragePath.Utilities
{
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Provides static utility members for interacting with physical path strings.
    /// </summary>
    internal static partial class PhysicalPathHelper
    {
        internal const char ExtensionSeparatorChar = '.';
        internal const string CurrentDirectorySegment = ".";
        internal const string ParentDirectorySegment = "..";

        internal static readonly char[] InvalidNewNameCharacters =
            new[]
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar,
                Path.VolumeSeparatorChar,
            }
            .Distinct()
            .ToArray();

        internal static readonly PathInformation PhysicalPathInformation = new PathInformation(
            Path.GetInvalidPathChars(),
            Path.GetInvalidFileNameChars(),
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
            extensionSeparatorChar: ExtensionSeparatorChar,
            Path.VolumeSeparatorChar,
            currentDirectorySegment: CurrentDirectorySegment,
            parentDirectorySegment: ParentDirectorySegment,
            defaultStringComparison: StringComparison
        );

        internal static string? GetExtensionWithoutTrailingExtensionSeparator(string? path)
        {
            return TrimTrailingExtensionSeparator(Path.GetExtension(path));

            static string? TrimTrailingExtensionSeparator(string? extension)
            {
                if (string.IsNullOrEmpty(extension) || extension![0] != ExtensionSeparatorChar)
                {
                    return extension;
                }
                return extension.Substring(1);
            }
        }
    }
}
