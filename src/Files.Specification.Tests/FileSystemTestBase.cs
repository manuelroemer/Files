namespace Files.Specification.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using Files;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

    /// <summary>
    ///     A base for test classes which need to interact or test a file system element
    ///     of some kind.
    ///     Using this base class provides access to a <see cref="FileSystemTestContext"/> and
    ///     thus locations where a specific <see cref="FileSystem"/> can be tested.
    /// </summary>
    [TestClass]
    public abstract class FileSystemTestBase
    {
        private StorageFolder? _testFolder;

        /// <summary>
        ///     Gets the <see cref="FileSystemTestContext"/> with which the test class has been initialized.
        /// </summary>
        protected FileSystemTestContext Context { get; }

        /// <summary>
        ///     Gets the value of the <see cref="FileSystemTestContext.FileSystem"/> property of the
        ///     <see cref="Context"/>. This simply exists for quickly accessing the file system.
        /// </summary>
        protected FileSystem FileSystem => Context.FileSystem;

        /// <summary>
        ///     Gets the value of the <see cref="FileSystem.PathInformation"/> property.
        ///     This simply exists for quickly accessing the file system.
        /// </summary>
        protected PathInformation PathInformation => Context.FileSystem.PathInformation;

        /// <summary>
        ///     Gets a folder in which all file system tests should operate.
        ///     By only interacting with items in this folder, the tests are deterministic.
        /// </summary>
        [AllowNull]
        protected StorageFolder TestFolder
        {
            get => _testFolder ?? throw new InvalidOperationException("The test folder has not been initialized yet.");
            set => _testFolder = value;
        }

        /// <summary>
        ///     Gets the root folder which contains the <see cref="TestFolder"/>.
        ///     No I/O operations should be performed in this folder. Restrict tests on the root folder
        ///     on read-only tests.
        /// </summary>
        protected StorageFolder RootFolder => 
            FileSystem.GetFolder(
                TestFolder.Path.FullPath.Root ??
                throw new InvalidOperationException("Could not retrieve a root folder from the test folder path.")
            );

        /// <summary>
        ///     Initializes a new <see cref="FileSystemTestBase"/> instance using the specified
        ///     <paramref name="context"/> for testing a file system.
        /// </summary>
        /// <param name="context">The context to be used for testing a file system.</param>
        public FileSystemTestBase(FileSystemTestContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TestFolder = null!; // That's good enough for the tests.
        }

        /// <summary>
        ///     Initializes the isolated <see cref="TestFolder"/>, resulting in a clean and empty state.
        /// </summary>
        [TestInitialize]
        public virtual async Task Initialize()
        {
            // If the tests got aborted before, a cleanup may be necessary.
            await Cleanup().ConfigureAwait(false);

            TestFolder = await Context.GetTestFolderAsync().ConfigureAwait(false);
            await TestFolder.CreateAsync(CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
        }

        /// <summary>
        ///     Deletes the isolated <see cref="TestFolder"/>.
        /// </summary>
        [TestCleanup]
        public virtual async Task Cleanup()
        {
            try
            {
                if (_testFolder is object)
                {
                    await LogFinalTestFolderStateAsync();
                    await _testFolder.DeleteAsync(DeletionOption.IgnoreMissing);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Should not be thrown, but it MAY happen if the underlying implementation is wrong.
            }
        }

        private async Task LogFinalTestFolderStateAsync()
        {
            try
            {
                Logger.LogMessage("Final test folder state:");
                if (!await TestFolder.ExistsAsync())
                {
                    Logger.LogMessage("The test folder does not exist anymore.");
                    return;
                }

                await LogRecursively(TestFolder);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Logging the state of the test folder failed with an exception: {ex.ToString()}");
            }

            static async Task LogRecursively(StorageFolder folder, int level = 0)
            {
                LogFsMember(folder, level);

                foreach (var file in await folder.GetAllFilesAsync())
                {
                    LogFsMember(file, level + 2);
                }

                foreach (var subFolder in await folder.GetAllFoldersAsync())
                {
                    await LogRecursively(subFolder, level + 2);
                }
            }

            static void LogFsMember(StorageElement element, int level)
            {
                var emojiPrefix = element is StorageFile ? "📄" : "📁";
                var levelWhitespace = new string(' ', level);
                var msg = $"{levelWhitespace}|_ {emojiPrefix} {element.Path.Name}";
                Logger.LogMessage($"{msg}");
            }
        }
    }
}
