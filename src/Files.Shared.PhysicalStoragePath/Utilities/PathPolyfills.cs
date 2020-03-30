// Parts of this file are copied from the source files of the dotnet/runtime repository and adapted/enhanced.
// The relevant file can be found at:
// https://github.com/dotnet/runtime/tree/master/src/libraries/System.Private.CoreLib/src/System/IO
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
#if NETSTANDARD2_1 || NETCOREAPP2_2 || NETCOREAPP2_1
        private const int WindowsDevicePrefixLength = 4;
        private const int WindowsUncPrefixLength = 2;
        private const int WindowsUncExtendedPrefixLength = 8;
#endif

#if NETSTANDARD2_1 || NETCOREAPP2_2 || NETCOREAPP2_1
        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.cs#L21
        internal static bool EndsInDirectorySeparator(string path)
        {
            return path.Length != 0 && WindowsIsDirectorySeparator(path[path.Length - 1]);
        }
#else
        internal static bool EndsInDirectorySeparator(string path)
        {
            return Path.EndsInDirectorySeparator(path);
        }
#endif

#if NETSTANDARD2_1 || NETCOREAPP2_2 || NETCOREAPP2_1
        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.cs#L226
        internal static string TrimEndingDirectorySeparator(string path)
        {
            if (EndsInDirectorySeparator(path) && !IsRoot(path))
            {
                return path.Substring(0, path.Length - 1);
            }
            else
            {
                return path;
            }
        }
#else
        internal static string TrimEndingDirectorySeparator(string path)
        {
            return Path.TrimEndingDirectorySeparator(path);
        }
#endif

#if NETSTANDARD2_1 || NETCOREAPP2_2 || NETCOREAPP2_1
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

                if ((!deviceSyntax || deviceUnc) && pathLength > 0 && WindowsIsDirectorySeparator(path[0]))
                {
                    // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                    if (deviceUnc || (pathLength > 1 && WindowsIsDirectorySeparator(path[1])))
                    {
                        // UNC (\\?\UNC\ or \\), scan past server\share

                        // Start past the prefix ("\\" or "\\?\UNC\")
                        i = deviceUnc ? WindowsUncExtendedPrefixLength : WindowsUncPrefixLength;

                        // Skip two separators at most
                        var n = 2;
                        while (i < pathLength && (!WindowsIsDirectorySeparator(path[i]) || --n > 0))
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
                    while (i < pathLength && !WindowsIsDirectorySeparator(path[i]))
                        i++;

                    // If there is another separator take it, as long as we have had at least one
                    // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                    if (i < pathLength && i > WindowsDevicePrefixLength && WindowsIsDirectorySeparator(path[i]))
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
                    if (pathLength > 2 && WindowsIsDirectorySeparator(path[2]))
                        i++;
                }

                return i;
            }

            // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Unix.cs#L22
            static int UnixImpl(string path)
            {
                return path.Length > 0 && WindowsIsDirectorySeparator(path[0]) ? 1 : 0;
            }
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L301
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool WindowsIsDirectorySeparator(char c)
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
                    && WindowsIsDirectorySeparator(path[0])
                    && WindowsIsDirectorySeparator(path[1])
                    && (path[2] == '.' || path[2] == '?')
                    && WindowsIsDirectorySeparator(path[3])
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
                && WindowsIsDirectorySeparator(path[7])
                && path[4] == 'U'
                && path[5] == 'N'
                && path[6] == 'C';
        }

        // See https://github.com/dotnet/runtime/blob/f30675618fc379e112376acc6f1efa53733ee881/src/libraries/System.Private.CoreLib/src/System/IO/PathInternal.Windows.cs#L70
        private static bool WindowsIsValidDriveChar(char value)
        {
            return (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');
        }
#endif
    }
}
