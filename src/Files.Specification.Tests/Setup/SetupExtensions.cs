namespace Files.Specification.Tests.Setup
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Files;

    /// <summary>
    ///     Defines static extension methods for conveniently setting up file system structures
    ///     with a focus on simplifying the test cases.
    /// </summary>
    public static class SetupExtensions
    {
        /// <summary>
        ///     Recursively creates and returns a file relative to the specified folder, replacing
        ///     previously existing files and folders, but instead of the file, returns a
        ///     <see cref="StorageFolder"/> pointing to the newly created location, therefore
        ///     allowing simple type conflict tests.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified folder's path, thus
        ///     forming the path where the new file is ultimately located.
        /// </param>
        public static Task<StorageFolder> SetupFileAndGetFolderAtSameLocation(
            this StorageFolder folder, params string[] pathSegments
        )
        {
            return folder.SetupFileAndGetFolderAtSameLocation(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Recursively creates and returns a file relative to the specified folder, replacing
        ///     previously existing files and folders, but instead of the file, returns a
        ///     <see cref="StorageFolder"/> pointing to the newly created location, therefore
        ///     allowing simple type conflict tests.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified folder's path, builds and returns a new
        ///     path where the new file should be created.
        /// </param>
        public static async Task<StorageFolder> SetupFileAndGetFolderAtSameLocation(
            this StorageFolder folder, PathProvider pathProvider
        )
        {
            await folder.SetupFileAsync(pathProvider);
            return folder.GetFolder(pathProvider);
        }

        /// <summary>
        ///     Recursively creates and returns a folder relative to the specified folder, replacing
        ///     previously existing folders, but instead of the folder, returns a
        ///     <see cref="StorageFile"/> pointing to the newly created location, therefore
        ///     allowing simple type conflict tests.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified folder's path, thus
        ///     forming the path where the new folder is ultimately located.
        /// </param>
        public static Task<StorageFile> SetupFolderAndGetFileAtSameLocation(
            this StorageFolder folder, params string[] pathSegments
        )
        {
            return folder.SetupFolderAndGetFileAtSameLocation(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Recursively creates and returns a folder relative to the specified folder, replacing
        ///     previously existing folders, but instead of the folder, returns a
        ///     <see cref="StorageFile"/> pointing to the newly created location, therefore
        ///     allowing simple type conflict tests.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified folder's path, builds and returns a new
        ///     path where the new folder should be created.
        /// </param>
        public static async Task<StorageFile> SetupFolderAndGetFileAtSameLocation(
            this StorageFolder folder, PathProvider pathProvider
        )
        {
            await folder.SetupFolderAsync(pathProvider);
            return folder.GetFile(pathProvider);
        }

        /// <summary>
        ///     Returns a file relative to the specified folder's path.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified folder's path, thus
        ///     forming the path where the file to be retrieved is ultimately located.
        /// </param>
        public static StorageFile GetFile(this StorageFolder folder, params string[] pathSegments)
        {
            return folder.GetFile(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Returns a file relative to the specified folder's path.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified folder's path, builds and returns a new
        ///     path where the file to be retrieved is located.
        /// </param>
        public static StorageFile GetFile(this StorageFolder folder, PathProvider pathProvider)
        {
            var filePath = folder.GetPath(pathProvider);
            return folder.FileSystem.GetFile(filePath);
        }

        /// <summary>
        ///     Returns a folder relative to the specified folder's path.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified folder's path, thus
        ///     forming the path where the folder to be retrieved is ultimately located.
        /// </param>
        public static StorageFolder GetFolder(this StorageFolder folder, params string[] pathSegments)
        {
            return folder.GetFolder(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Returns a folder relative to the specified folder's path.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified folder's path, builds and returns a new
        ///     path where the folder to be retrieved is located.
        /// </param>
        public static StorageFolder GetFolder(this StorageFolder folder, PathProvider pathProvider)
        {
            var folderPath = folder.GetPath(pathProvider);
            return folder.FileSystem.GetFolder(folderPath);
        }

        /// <summary>
        ///     Recursively creates and returns a file relative to the specified folder, replacing
        ///     previously existing files and folders.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified folder's path, thus
        ///     forming the path where the new file is ultimately located.
        /// </param>
        /// <example>
        ///     <code>
        ///     var folder = fs.GetFolder("basePath");
        ///     var file = folder.SetupFileAsync("foo", "bar.baz");
        ///     // Path: basePath/foo/bar.baz
        ///     </code>
        /// </example>
        public static Task<StorageFile> SetupFileAsync(this StorageFolder folder, params string[] pathSegments)
        {
            return folder.SetupFileAsync(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Recursively creates and returns a file relative to the specified folder, replacing
        ///     previously existing files and folders.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified folder's path, builds and returns a new
        ///     path where the new file should be created.
        /// </param>
        /// <example>
        ///     <code>
        ///     var folder = fs.GetFolder("basePath");
        ///     var file = folder.SetupFileAsync(basePath => basePath / "foo" / "bar.baz");
        ///     // Path: basePath/foo/bar.baz
        ///     </code>
        /// </example>
        public static async Task<StorageFile> SetupFileAsync(this StorageFolder folder, PathProvider pathProvider)
        {
            var newFilePath = folder.GetPath(pathProvider);
            var newFile = folder.FileSystem.GetFile(newFilePath);
            await newFile.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting);
            return newFile;
        }

        /// <summary>
        ///     Recursively creates and returns a folder relative to the specified folder, replacing
        ///     previously existing folders.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified folder's path, thus
        ///     forming the path where the new folder is ultimately located.
        /// </param>
        /// <example>
        ///     <code>
        ///     var folder = fs.GetFolder("basePath");
        ///     var file = folder.SetupFolderAsync("foo", "bar");
        ///     // Path: basePath/foo/bar
        ///     </code>
        /// </example>
        public static Task<StorageFolder> SetupFolderAsync(this StorageFolder folder, params string[] pathSegments)
        {
            return folder.SetupFolderAsync(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Recursively creates and returns a folder relative to the specified folder, replacing
        ///     previously existing folders.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified folder's path, builds and returns a new
        ///     path where the new folder should be created.
        /// </param>
        /// <example>
        ///     <code>
        ///     var folder = fs.GetFolder("basePath");
        ///     var file = folder.SetupFolderAsync(basePath => basePath / "foo" / "bar");
        ///     // Path: basePath/foo/bar
        ///     </code>
        /// </example>
        public static async Task<StorageFolder> SetupFolderAsync(this StorageFolder folder, PathProvider pathProvider)
        {
            var newFolderPath = folder.GetPath(pathProvider);
            var newFolder = folder.FileSystem.GetFolder(newFolderPath);
            await newFolder.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting);
            return newFolder;
        }

        /// <summary>
        ///     Returns a path relative to the specified element's path.
        /// </summary>
        /// <param name="pathSegments">
        ///     An array of path segments which get joined with the specified element's path, thus
        ///     forming the path to be returned.
        /// </param>
        public static StoragePath GetPath(this StorageElement element, params string[] pathSegments)
        {
            return element.GetPath(basePath => SegmentsToPath(basePath, pathSegments));
        }

        /// <summary>
        ///     Returns a path relative to the specified element's path.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which, by receiving the specified element's path, builds and returns the
        ///     path to be returned.
        /// </param>
        public static StoragePath GetPath(this StorageElement element, PathProvider pathProvider)
        {
            return pathProvider.Invoke(element.Path.FullPath);
        }

        private static StoragePath SegmentsToPath(StoragePath basePath, IEnumerable<string> pathSegments)
        {
            return pathSegments.Aggregate(basePath, (currentPath, segment) => currentPath / segment);
        }
    }
}
