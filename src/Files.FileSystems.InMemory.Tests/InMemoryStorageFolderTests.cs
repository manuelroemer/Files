namespace Files.FileSystems.InMemory.Tests
{
    using System.IO;
    using System.Threading.Tasks;
    using Files.Specification.Tests;
    using Files.Specification.Tests.Assertions;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public sealed class InMemoryStorageFolderTests : StorageFolderSpecificationTests
    {
        public InMemoryStorageFolderTests()
            : base(InMemoryFileSystemTestContext.Instance) { }

        #region Locked Folder Tests

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task MethodsMutatingFolderLocationOrContent_LockedNestedFile_ShouldThrowIOException(FileAccess fileAccess)
        {
            var folder = await TestFolder.SetupFolderAsync(Default.FolderName);
            var nestedFile = await folder.SetupFileAsync(Default.FileName);
            var dstParentFolder = await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);

            using (await nestedFile.OpenAsync(fileAccess))
            {
                await Should.ThrowAsync<IOException>(async () => await folder.CreateAsync(CreationCollisionOption.ReplaceExisting));
                await Should.ThrowAsync<IOException>(async () => await folder.DeleteAsync());
                await Should.ThrowAsync<IOException>(async () => await folder.CopyAsync(dstFilePath));
                await Should.ThrowAsync<IOException>(async () => await folder.MoveAsync(dstFilePath));
                await folder.ShouldExistAsync();
            }
        }

        #endregion
    }
}
