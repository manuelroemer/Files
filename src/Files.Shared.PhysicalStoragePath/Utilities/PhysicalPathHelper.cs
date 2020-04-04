namespace Files.Shared.PhysicalStoragePath.Utilities
{
    using System;
    using System.Globalization;
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
                // Used for the newName parameter in methods like RenameAsync.
                // In essence, we want to avoid names like "foo/bar", i.e. relative paths.
                // We simply forbid directory separator chars in the name to achieve that.
                // Also forbid the volume separator, because it might also be a /.
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

        internal static string GetTemporaryElementName()
        {
            var guidPart = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            return $"{guidPart}~tmp";
        }
    }
}
