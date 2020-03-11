namespace Files.Specification.Tests.Assertions
{
    using System.Text;
    using System.Threading.Tasks;
    using Shouldly;

    public static class StorageFileAssertions
    {

        public static async Task ShouldHaveContentAsync(this StorageFile file, string expectedContent, Encoding? encoding = null)
        {
            var content = await file.ReadTextAsync(encoding).ConfigureAwait(false);
            content.ShouldBe(expectedContent);
        }

        public static async Task ShouldNotHaveContentAsync(this StorageFile file, string unexpectedContent, Encoding? encoding = null)
        {
            var content = await file.ReadTextAsync(encoding).ConfigureAwait(false);
            content.ShouldNotBe(unexpectedContent);
        }

        public static async Task ShouldHaveContentAsync(this StorageFile file, byte[] expectedContent)
        {
            var content = await file.ReadBytesAsync();
            content.ShouldBe(expectedContent);
        }

        public static async Task ShouldNotHaveContentAsync(this StorageFile file, byte[] unexpectedContent)
        {
            var content = await file.ReadBytesAsync();
            content.ShouldNotBe(unexpectedContent);
        }

        public static async Task ShouldHaveEmptyContentAsync(this StorageFile file)
        {
            var content = await file.ReadBytesAsync();
            content.ShouldBeEmpty();
        }

        public static async Task ShouldNotHaveEmptyContentAsync(this StorageFile file)
        {
            var content = await file.ReadBytesAsync();
            content.ShouldNotBeEmpty();
        }

    }

}
