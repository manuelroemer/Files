namespace Files.Specification.Tests.Utilities
{
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text;
    using StorageFile = StorageFile;

    /// <summary>
    ///     Extends the <see cref="Assert"/> class with file system specific assertions which
    ///     can be used via <see cref="Assert.That"/>.
    /// </summary>
    public static class FsAssert
    {

        public static void PathsAreEffectivelyEqual(this Assert _, StoragePath first, StoragePath second)
        {
            // Doing this assertion is rather hard since different paths could point to the same element.
            // GetFullPath() is a good first step to get some kind of normalized path though.
            // The afterwards comparison is supposed to handle casing depending on the FS.
            Assert.AreEqual(
                first.FullPath,
                second.FullPath,
                $"The two paths {first} and {second} do not seem to point to the same location, but should."
            );
        }
        
        public static void PathsAreNotEffectivelyEqual(this Assert _, StoragePath first, StoragePath second)
        {
            // Doing this assertion is rather hard since different paths could point to the same element.
            // GetFullPath() is a good first step to get some kind of normalized path though.
            // The afterwards comparison is supposed to handle casing depending on the FS.
            Assert.AreNotEqual(
                first.FullPath,
                second.FullPath,
                $"The two paths {first} and {second} seem to point to the same location, but shouldn't."
            );
        }

        public static async Task ElementExistsAsync(this Assert _, StorageElement element)
        {
            Assert.IsTrue(
                await element.ExistsAsync().ConfigureAwait(false),
                $"The element at {element.Path} doesn't exist, but it should."
            );
        }

        public static async Task ElementDoesNotExistAsync(this Assert _, StorageElement element)
        {
            Assert.IsFalse(
                await element.ExistsAsync().ConfigureAwait(false),
                $"The element at {element.Path} exists, but it shouldn't."
            );
        }

        public static async Task FilesHaveSameContentAsync(this Assert _, StorageFile first, StorageFile second)
        {
            var c1 = await first.ReadBytesAsync().ConfigureAwait(false);
            var c2 = await second.ReadBytesAsync().ConfigureAwait(false);
            Assert.IsTrue(
                c1.SequenceEqual(c2),
                $"The contents of the two files at {first.Path} and {second.Path} are not equal, but should be."
            );
        }

        public static async Task FilesDoNotHaveSameContentAsync(this Assert _, StorageFile first, StorageFile second)
        {
            var c1 = await first.ReadBytesAsync().ConfigureAwait(false);
            var c2 = await second.ReadBytesAsync().ConfigureAwait(false);
            Assert.IsFalse(
                c1.SequenceEqual(c2),
                $"The content of the two files at {first.Path} and {second.Path} are equal, but shouldn't be."
            );
        }

        public static async Task FileHasContentAsync(
            this Assert _, StorageFile file, string expectedContent, Encoding? encoding = null)
        {
            var content = await file.ReadTextAsync(encoding).ConfigureAwait(false);
            Assert.AreEqual(
                expectedContent,
                content,
                $"The file at {file.Path} has the content '{content}' which is not equal to '{expectedContent}', but should be."
            );
        }

        public static async Task FileHasContentAsync(
            this Assert _, StorageFile file, byte[] expectedContent)
        {
            var content = await file.ReadBytesAsync().ConfigureAwait(false);
            Assert.AreEqual(
                expectedContent,
                content,
                $"The file at {file.Path} has the content '{content}' which is not equal to '{expectedContent}', but should be."
            );
        }

        public static async Task FileDoesNotHaveContentAsync(
            this Assert _, StorageFile file, string expectedContent, Encoding? encoding = null)
        {
            var content = await file.ReadTextAsync(encoding).ConfigureAwait(false);
            Assert.AreNotEqual(
                expectedContent,
                content,
                $"The file at {file.Path} has the content '{content}' which is equal to '{expectedContent}', but shouldn't be."
            );
        }

        public static async Task FileDoesNotHaveContentAsync(
            this Assert _, StorageFile file, byte[] expectedContent)
        {
            var content = await file.ReadBytesAsync().ConfigureAwait(false);
            Assert.AreNotEqual(
                expectedContent,
                content,
                $"The file at {file.Path} has the content '{content}' which is equal to '{expectedContent}', but shouldn't be."
            );
        }

        public static async Task FolderIsEmpty(this Assert _, StorageFolder folder)
        {
            var children = await folder.GetAllChildrenAsync().ConfigureAwait(false);
            Assert.IsFalse(children.Any(), $"The folder at {folder.Path} is not empty, but should be.");
        }
        
        public static async Task FolderIsNotEmpty(this Assert _, StorageFolder folder)
        {
            var children = await folder.GetAllChildrenAsync().ConfigureAwait(false);
            Assert.IsTrue(children.Any(), $"The folder at {folder.Path} is empty, but shouldn't be.");
        }

    }

}
