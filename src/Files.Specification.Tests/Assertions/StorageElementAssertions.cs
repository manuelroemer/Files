namespace Files.Specification.Tests.Assertions
{
    using System.Threading.Tasks;
    using Shouldly;

    public static class StorageElementAssertions
    {
        public static async Task ShouldExistAsync(this StorageElement element)
        {
            var exists = await element.ExistsAsync();
            exists.ShouldBeTrue($"The element at {element.Path} should exist, but doesn't.");
        }

        public static async Task ShouldNotExistAsync(this StorageElement element)
        {
            var exists = await element.ExistsAsync();
            exists.ShouldBeFalse($"The element at {element.Path} should not exist, but does.");
        }
    }
}
