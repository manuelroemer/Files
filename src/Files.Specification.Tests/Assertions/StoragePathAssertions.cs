namespace Files.Specification.Tests.Assertions
{
    using Shouldly;

    public static class StoragePathAssertions
    {

        public static void ShouldBeEffectivelyEqualTo(this StoragePath? path, StoragePath? other)
        {
            PseudoNormalize(path).ShouldBe(PseudoNormalize(other));
        }
        
        public static void ShouldNotBeEffectivelyEqualTo(this StoragePath? path, StoragePath? other)
        {
            PseudoNormalize(path).ShouldNotBe(PseudoNormalize(other));
        }

        private static StoragePath? PseudoNormalize(StoragePath? path)
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
