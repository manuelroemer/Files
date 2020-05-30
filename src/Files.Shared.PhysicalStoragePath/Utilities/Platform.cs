namespace Files.Shared.PhysicalStoragePath.Utilities
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Provides values indicating on which platform the application is currently running.
    ///     
    ///     To ensure that the information is as accurate as possible, this class uses multiple
    ///     approaches for determining the platform.
    /// </summary>
    internal static class Platform
    {
        public static PlatformID? Current { get; } = GetCurrentPlatformID();

        private static PlatformID? GetCurrentPlatformID()
        {
            return GetViaRuntimeInformation()
                ?? GetViaEnvironment()
                ?? null;

            static PlatformID? GetViaRuntimeInformation()
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return PlatformID.Win32NT;
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        return PlatformID.Unix;
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        return PlatformID.Unix;
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("FREEBSD")))
                    {
                        return PlatformID.Unix;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }

            static PlatformID? GetViaEnvironment()
            {
                try
                {
                    // The switch might seem irrelevant, but we want to cover different cases not in .NET Standard.
                    return Environment.OSVersion.Platform switch
                    {
                        PlatformID.Win32NT => PlatformID.Win32NT,
                        PlatformID.Unix => PlatformID.Unix,
                        _ => null,
                    };
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
