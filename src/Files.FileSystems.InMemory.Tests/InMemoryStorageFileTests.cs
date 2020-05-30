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
    public sealed class InMemoryStorageFileTests : StorageFileSpecificationTests
    {
        public InMemoryStorageFileTests()
            : base(InMemoryFileSystemTestContext.Instance) { }

        #region OpenAsync Tests

        [TestMethod]
        [DataRow(FileAccess.Read, FileAccess.Read)]
        [DataRow(FileAccess.Read, FileAccess.Write)]
        [DataRow(FileAccess.Read, FileAccess.ReadWrite)]
        [DataRow(FileAccess.Write, FileAccess.Read)]
        [DataRow(FileAccess.Write, FileAccess.Write)]
        [DataRow(FileAccess.Write, FileAccess.ReadWrite)]
        [DataRow(FileAccess.ReadWrite, FileAccess.Read)]
        [DataRow(FileAccess.ReadWrite, FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite, FileAccess.ReadWrite)]
        public async Task OpenAsync_ConflictingOpenRequests_ThrowsIOException(FileAccess firstFileAccess, FileAccess secondFileAccess)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            using var stream1 = await file.OpenAsync(firstFileAccess);
            await Should.ThrowAsync<IOException>(async () => await file.OpenAsync(secondFileAccess));
        }

        #endregion

        #region Locked File Tests

        [TestMethod]
        [DataRow(FileAccess.Read)]
        [DataRow(FileAccess.Write)]
        [DataRow(FileAccess.ReadWrite)]
        public async Task MethodsMutatingFileLocationOrContent_LockedFile_ShouldThrowIOException(FileAccess fileAccess)
        {
            var file = await TestFolder.SetupFileAsync(Default.FileName);
            var dstParentFolder = await TestFolder.SetupFolderAsync(Default.DstParentFolderName);
            var dstFilePath = TestFolder.GetPath(Default.DstFileSegments);

            using (await file.OpenAsync(fileAccess))
            {
                await Should.ThrowAsync<IOException>(async () => await file.CreateAsync(CreationCollisionOption.ReplaceExisting));
                await Should.ThrowAsync<IOException>(async () => await file.DeleteAsync());
                await Should.ThrowAsync<IOException>(async () => await file.CopyAsync(dstFilePath));
                await Should.ThrowAsync<IOException>(async () => await file.MoveAsync(dstFilePath));
                await Should.ThrowAsync<IOException>(async () => await file.OpenAsync(FileAccess.Write));
                await Should.ThrowAsync<IOException>(async () => await file.WriteBytesAsync(Default.ByteContent));
                await Should.ThrowAsync<IOException>(async () => await file.WriteTextAsync(Default.TextContent));
                await file.ShouldExistAsync();
            }
        }

        #endregion
    }
}
