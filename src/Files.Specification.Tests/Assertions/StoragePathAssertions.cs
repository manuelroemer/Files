namespace Files.Specification.Tests.Assertions
{
    using Shouldly;

    public static class StoragePathAssertions
    {
        public static void ShouldBeWithNormalizedPathSeparators(this StoragePath? path, string? other)
        {
            var first = NormalizePathSeparators(path?.ToString(), path?.FileSystem.PathInformation);
            var second = NormalizePathSeparators(other, path?.FileSystem.PathInformation);
            first.ShouldBe(second);
        }
        
        public static void ShouldNotBeWithNormalizedPathSeparators(this StoragePath? path, string? other)
        {
            var first = NormalizePathSeparators(path?.ToString(), path?.FileSystem.PathInformation);
            var second = NormalizePathSeparators(other, path?.FileSystem.PathInformation);
            first.ShouldNotBe(second);
        }

        private static string? NormalizePathSeparators(string? str, PathInformation? pathInformation)
        {
            if (str is null || pathInformation is null)
            {
                return null;
            }
            return str.Replace(pathInformation.AltDirectorySeparatorChar, pathInformation.DirectorySeparatorChar);
        }

        public static void ShouldBeEffectivelyEqualTo(this StoragePath? path, StoragePath? other)
        {
            NormalizeFull(path).ShouldBe(NormalizeFull(other));
        }
        
        public static void ShouldNotBeEffectivelyEqualTo(this StoragePath? path, StoragePath? other)
        {
            NormalizeFull(path).ShouldNotBe(NormalizeFull(other));
        }

        private static StoragePath? NormalizeFull(StoragePath? path)
        {
            // We cannot really normalize the paths since there is no corresponding method.
            // The best we can do is to grab a full path and then remove any trailing separators.
            // This way, we should arrive at two equal paths in a lot of cases, at least.
            if (path is null)
            {
                return null;
            }

            var fullPath = path.FullPath;
            if (fullPath.TryTrimEndingDirectorySeparator(out var trimmedPath))
            {
                return trimmedPath;
            }
            else
            {
                return fullPath;
            }
        }
    }
}
