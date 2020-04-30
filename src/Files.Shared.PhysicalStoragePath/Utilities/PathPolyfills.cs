// Parts of this file are copied from the source files of the dotnet/runtime repository and adapted/enhanced.
// The relevant files can be found at:
// https://github.com/dotnet/runtime
//
// The code is licensed by the .NET Foundation with the following license header:
// > Licensed to the .NET Foundation under one or more agreements.
// > The .NET Foundation licenses this file to you under the MIT license.
// > See the LICENSE file in the project root for more information.

namespace Files.Shared.PhysicalStoragePath.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal static class PathPolyfills
    {
#if NETCOREAPP2_0 || NETSTANDARD2_0 || UAP
        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/Path.cs#L419
        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/Path.cs#L643
        internal static string Join(string path1, string path2)
        {
            if (path1.Length == 0)
            {
                return path2;
            }
            
            if (path2.Length == 0)
            {
                return path1;
            }

            var hasSeparator = IsDirectorySeparator(path1[path1.Length - 1]) || IsDirectorySeparator(path2[0]);
            return hasSeparator
                ? $"{path1}{path2}"
                : $"{path1}{Path.DirectorySeparatorChar}{path2}";
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/Path.cs#L282
        internal static bool IsPathFullyQualified(string path)
        {
            return !IsPartiallyQualified(path);

            static bool IsPartiallyQualified(string path)
            {
                return Platform.Current switch
                {
                    PlatformID.Win32NT => WindowsImpl(path),
                    PlatformID.Unix => UnixImpl(path),
                    _ => throw new PlatformNotSupportedException(),
                };

                // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L271
                static bool WindowsImpl(string path)
                {
                    if (path.Length < 2)
                    {
                        // It isn't fixed, it must be relative.  There is no way to specify a fixed
                        // path with one character (or less).
                        return true;
                    }

                    if (IsDirectorySeparator(path[0]))
                    {
                        // There is no valid way to specify a relative path with two initial slashes or
                        // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
                        return !(path[1] == '?' || IsDirectorySeparator(path[1]));
                    }

                    // The only way to specify a fixed path that doesn't begin with two slashes
                    // is the drive, colon, slash format- i.e. C:\
                    return !((path.Length >= 3)
                        && (path[1] == Path.VolumeSeparatorChar)
                        && IsDirectorySeparator(path[2])
                        // To match old behavior we'll check the drive character for validity as the path is technically
                        // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
                        && WindowsIsValidDriveChar(path[0]));
                }

                // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Unix.cs#L77
                static bool UnixImpl(string path)
                {
                    return !Path.IsPathRooted(path);
                }
            }
        }
#else
        internal static string Join(string path1, string path2)
        {
            return Path.Join(path1, path2);
        }

        internal static bool IsPathFullyQualified(string path)
        {
            return Path.IsPathFullyQualified(path);
        }
#endif

#if NETSTANDARD2_1 || NETCOREAPP2_2 || NETCOREAPP2_1 || NETCOREAPP2_0 || NETSTANDARD2_0 || UAP
        private const int WindowsDevicePrefixLength = 4;
        private const int WindowsUncPrefixLength = 2;
        private const int WindowsUncExtendedPrefixLength = 8;

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.cs#L21
        internal static bool EndsWithDirectorySeparator(string path)
        {
            return path.Length != 0 && IsDirectorySeparator(path[path.Length - 1]);
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.cs#L226
        internal static string TrimEndingDirectorySeparator(string path)
        {
            if (EndsWithDirectorySeparator(path) && !IsRoot(path))
            {
                return path.Substring(0, path.Length - 1);
            }
            else
            {
                return path;
            }
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.cs#L28
        private static bool IsRoot(string path)
        {
            return path.Length == GetRootLength(path);
        }

        private static int GetRootLength(string path)
        {
            return Platform.Current switch
            {
                PlatformID.Win32NT => WindowsImpl(path),
                PlatformID.Unix => UnixImpl(path),
                _ => throw new PlatformNotSupportedException(),
            };

            // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L202
            static int WindowsImpl(string path)
            {
                var pathLength = path.Length;
                var i = 0;

                var deviceSyntax = IsDevice(path);
                var deviceUnc = deviceSyntax && WindowsIsDeviceUNC(path);

                if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
                {
                    // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                    if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                    {
                        // UNC (\\?\UNC\ or \\), scan past server\share

                        // Start past the prefix ("\\" or "\\?\UNC\")
                        i = deviceUnc ? WindowsUncExtendedPrefixLength : WindowsUncPrefixLength;

                        // Skip two separators at most
                        var n = 2;
                        while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                            i++;
                    }
                    else
                    {
                        // Current drive rooted (e.g. "\foo")
                        i = 1;
                    }
                }
                else if (deviceSyntax)
                {
                    // Device path (e.g. "\\?\.", "\\.\")
                    // Skip any characters following the prefix that aren't a separator
                    i = WindowsDevicePrefixLength;
                    while (i < pathLength && !IsDirectorySeparator(path[i]))
                        i++;

                    // If there is another separator take it, as long as we have had at least one
                    // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                    if (i < pathLength && i > WindowsDevicePrefixLength && IsDirectorySeparator(path[i]))
                        i++;
                }
                else if (
                       pathLength >= 2
                    && path[1] == Path.VolumeSeparatorChar
                    && WindowsIsValidDriveChar(path[0])
                )
                {
                    // Valid drive specified path ("C:", "D:", etc.)
                    i = 2;

                    // If the colon is followed by a directory separator, move past it (e.g "C:\")
                    if (pathLength > 2 && IsDirectorySeparator(path[2]))
                        i++;
                }

                return i;
            }

            // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Unix.cs#L22
            static int UnixImpl(string path)
            {
                return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
            }
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L301
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirectorySeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L132
        private static bool IsDevice(string path)
        {
            // If the path begins with any two separators is will be recognized and normalized and prepped with
            // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
            return WindowsIsExtended(path) ||
                (
                       path.Length >= WindowsDevicePrefixLength
                    && IsDirectorySeparator(path[0])
                    && IsDirectorySeparator(path[1])
                    && (path[2] == '.' || path[2] == '?')
                    && IsDirectorySeparator(path[3])
                );
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L165
        private static bool WindowsIsExtended(string path)
        {
            // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
            // Skipping of normalization will *only* occur if back slashes ('\') are used.
            return path.Length >= WindowsDevicePrefixLength
                && path[0] == '\\'
                && (path[1] == '\\' || path[1] == '?')
                && path[2] == '?'
                && path[3] == '\\';
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L150
        private static bool WindowsIsDeviceUNC(string path)
        {
            return path.Length >= WindowsUncExtendedPrefixLength
                && IsDevice(path)
                && IsDirectorySeparator(path[7])
                && path[4] == 'U'
                && path[5] == 'N'
                && path[6] == 'C';
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L70
        private static bool WindowsIsValidDriveChar(char value)
        {
            return (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');
        }
#else
        internal static bool EndsWithDirectorySeparator(string path)
        {
            return Path.EndsInDirectorySeparator(path);
        }

        internal static string TrimEndingDirectorySeparator(string path)
        {
            return Path.TrimEndingDirectorySeparator(path);
        }
#endif
    }
}
