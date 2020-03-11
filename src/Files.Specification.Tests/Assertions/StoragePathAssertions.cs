namespace Files.Specification.Tests.Assertions
{
    using Shouldly;

    public static class StoragePathAssertions
    {

        public static void ShouldBeEffectivelyEqualTo(this StoragePath? path, StoragePath? other)
        {
            path?.FullPath.ShouldBe(other?.FullPath);
        }
        
        public static void ShouldNotBeEffectivelyEqualTo(this StoragePath? path, StoragePath? other)
        {
            path?.FullPath.ShouldNotBe(other?.FullPath);
        }

    }

}
