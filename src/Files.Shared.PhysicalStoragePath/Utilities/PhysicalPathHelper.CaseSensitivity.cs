// Parts of this file are copied from the source files of the dotnet/runtime repository and adapted/enhanced.
// The relevant file can be found at:
// https://github.com/dotnet/runtime/blob/8cc28d7ee63ae6f07dd9a37b1b8a935be565076e/src/libraries/Common/src/System/IO/PathInternal.CaseSensitivity.cs
// -or-
// https://source.dot.net/#System.IO.FileSystem/Common/System/IO/PathInternal.CaseSensitivity.cs
//
// The code is licensed by the .NET Foundation with the following license header:
// > Licensed to the .NET Foundation under one or more agreements.
// > The .NET Foundation licenses this file to you under the MIT license.
// > See the LICENSE file in the project root for more information.

namespace Files.Shared.PhysicalStoragePath.Utilities
{
    using System;
    using System.IO;

    // The .NET BCL doesn't provide us with a public API to determine whether the current FS is
    // case-sensitive or not.
    // Since we need that information in several parts of the library, we are using the same code
    // as the .NET Core team (see the top of this file, both for the source and for explanations).
    //
    // The code in this file should be updated if a better way/API ever becomes available.

    internal static partial class PhysicalPathHelper
    {
        internal static StringComparison StringComparison => IsCaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

#if UAP
        // For the WindowsStorage project.
        // Windows is always assumed to be case-insensitive.
        // Not doing the full check as below, since writing files is harder to do in a UWP sandbox.
        internal static bool IsCaseSensitive { get; } = false;
#else
        internal static bool IsCaseSensitive { get; } = GetIsCaseSensitive();

        private static bool GetIsCaseSensitive()
        {
            // The original .NET code creates a file with upper-case letters and tries to access it
            // with a lower-case filename. If this fails with an exception, they simply assume false.
            // This seems hacky, but it works. Furthermore, since we are exactly mimicing the .NET
            // behavior, we shouldn't run into too many troubles.
            // We enhance that logic with a look at the current platform, since we can usually assume
            // that Windows is case-insensitive, while Unix platforms are not.
            return GetViaTemporaryFile()
                ?? GetViaPlatform()
                ?? false;

            static bool? GetViaTemporaryFile()
            {
#pragma warning disable CA1305, CA1308
                try
                {
                    var pathWithUpperCase = Path.Combine(Path.GetTempPath(), "CASESENSITIVETEST" + Guid.NewGuid().ToString("N"));
                    using (new FileStream(pathWithUpperCase, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 0x1000, FileOptions.DeleteOnClose))
                    {
                        var lowerCased = pathWithUpperCase.ToLowerInvariant();
                        return !File.Exists(lowerCased);
                    }
                }
                catch
                {
                    return null;
                }
#pragma warning restore
            }

            static bool? GetViaPlatform()
            {
                return Platform.Current switch
                {
                    PlatformID.Win32NT => false,
                    PlatformID.Unix => true,
                    _ => null,
                };
            }
        }
#endif
    }
}
