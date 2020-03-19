namespace Files.Specification.Tests.Setup
{
    using System.Collections.Generic;
    using System.IO.MemoryMappedFiles;
    using System.Linq;
    using System.Threading.Tasks;
    using Files;

    /// <summary>
    ///     Defines static extension methods for conveniently setting up common file system structures
    ///     which are frequently used in tests.
    ///     These structures use the names defined in <see cref="Default"/>.
    /// </summary>
    public static class SetupExtensions
    {

        /// <summary>
        ///     Returns a non-existing folder whose parent folder is also missing.
        /// </summary>
        public static StorageFolder GetFolderWithNonExistingParent(this StorageFolder folder)
        {
            return folder.GetFolder(Default.FolderName, Default.FolderName);
        }
        
        /// <summary>
        ///     Returns a non-existing folder whose parent folder is also missing.
        /// </summary>
        public static StorageFile GetFileWithNonExistingParent(this StorageFolder folder)
        {
            return folder.GetFile(Default.FolderName, Default.FileName);
        }

        /// <summary>
        ///     Builds and returns a path to the <see cref="Default.DstParentFolderName"/>, 
        ///     relative to the current folder.
        ///     
        ///     The path looks similar to this: <c>folder/dst</c>
        /// </summary>
        public static StoragePath GetDstParentFolderPath(this StorageFolder folder)
        {
            return folder.Path.FullPath / Default.DstParentFolderName;
        }

        /// <summary>
        ///     Builds and returns a path to the <see cref="Default.DstFileName"/> contained within 
        ///     the <see cref="Default.DstParentFolderName"/>, 
        ///     relative to the current folder.
        ///     
        ///     The path looks similar to this: <c>folder/dst/dstFile.ext</c>
        /// </summary>
        public static StoragePath GetDstFilePath(this StorageFolder folder)
        {
            return folder.GetDstParentFolderPath() / Default.DstFileName;
        }

        /// <summary>
        ///     Sets up a default folder structure for testing copy and move operations, i.e. a
        ///     source and destination folder and a file within the source folder that can be
        ///     copied or moved.
        ///     
        ///     The final structure looks similar to this:
        ///     
        ///     <code>
        ///     folder
        ///     |_ src
        ///     |  |_ srcFile.ext
        ///     |_ dst
        ///     </code>
        /// </summary>
        public static async Task<(StorageFolder SrcParentFolder, StorageFolder DstParentFolder, StorageFile SrcFile)> SetupSrcDstForFileAsync(this StorageFolder folder)
        {
            return (
                await folder.SetupFolderAsync(Default.SrcParentFolderName),
                await folder.SetupFolderAsync(Default.DstParentFolderName),
                await folder.SetupFileAsync(Default.SrcParentFolderName, Default.SrcFileName)
            );
        }

        /// <summary>
        ///     Sets up a default folder structure for testing copy and move operations, i.e. a
        ///     source and destination folder and a folder within the source folder that can be
        ///     copied or moved.
        ///     
        ///     The final structure looks similar to this:
        ///     
        ///     <code>
        ///     folder
        ///     |_ src
        ///     |  |_ srcFolder
        ///     |_ dst
        ///     </code>
        /// </summary>
        public static async Task<(StorageFolder SrcParentFolder, StorageFolder DstParentFolder, StorageFolder SrcFolder)> SetupSrcDstForFolderAsync(this StorageFolder folder)
        {
            return (
                await folder.SetupFolderAsync(Default.SrcParentFolderName),
                await folder.SetupFolderAsync(Default.DstParentFolderName),
                await folder.SetupFolderAsync(Default.SrcParentFolderName, Default.SrcFolderName)
            );
        }

        /// <summary>
        ///     Sets up a source folder containing a source file for testing copy and move operations.
        /// 
        ///     The final structure looks similar to this:
        ///     
        ///     <code>
        ///     folder
        ///     |_ src
        ///        |_ srcFile.ext
        ///     </code>
        /// </summary>
        public static async Task<(StorageFolder SrcParentFolder, StorageFile SrcFile)> SetupSrcFileAsync(this StorageFolder folder)
        {
            var srcParentFolder = await folder.SetupFolderAsync(Default.SrcParentFolderName);
            var srcFile = await srcParentFolder.SetupFileAsync(Default.SrcFileName);
            return (srcParentFolder, srcFile);
        }

        /// <summary>
        ///     Sets up two files which can be used for testing file conflicts using the <see cref="Default.FileName"/>
        ///     and <see cref="Default.ConflictingFileName"/> names.
        ///     
        ///     The final structure looks similar to this:
        ///     
        ///     <code>
        ///     folder
        ///     |_ srcFile.ext
        ///     |_ dstFile.ext
        ///     </code>
        /// </summary>
        public static async Task<(StorageFile SrcFile, StorageFile ConflictingDstFile)> SetupTwoConflictingFilesAsync(this StorageFolder folder)
        {
            return (
                await folder.SetupFileAsync(Default.FileName),
                await folder.SetupFileAsync(Default.ConflictingFileName)
            );
        }

        /// <summary>
        ///     Sets up a file and returns a folder which points to the same location as the created file.
        ///     This can be used for testing how APIs behave when an element of another type exists
        ///     at the same location.
        /// </summary>
        public static async Task<StorageFolder> SetupFileAndGetFolderAtSameLocation(this StorageFolder folder)
        {
            await folder.SetupFileAsync(Default.SharedFileFolderName);
            return folder.GetFolder(Default.SharedFileFolderName);
        }

        /// <summary>
        ///     Sets up a folder and returns a file which points to the same location as the created folder.
        ///     This can be used for testing how APIs behave when an element of another type exists
        ///     at the same location.
        /// </summary>
        public static async Task<StorageFile> SetupFolderAndGetFileAtSameLocation(this StorageFolder folder)
        {
            await folder.SetupFolderAsync(Default.SharedFileFolderName);
            return folder.GetFile(Default.SharedFileFolderName);
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
            return folder.GetFile(basePath => ToPath(basePath, pathSegments));
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
            var filePath = pathProvider.Invoke(folder.Path.FullPath);
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
            return folder.GetFolder(basePath => ToPath(basePath, pathSegments));
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
            var folderPath = pathProvider.Invoke(folder.Path.FullPath);
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
            return folder.SetupFileAsync(basePath => 
                pathSegments.Aggregate(basePath, (currentPath, segment) => currentPath / segment)
            );
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
            var newFilePath = pathProvider.Invoke(folder.Path.FullPath);
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
            return folder.SetupFolderAsync(basePath =>
                pathSegments.Aggregate(basePath, (currentPath, segment) => currentPath / segment)
            );
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
            var newFolderPath = pathProvider.Invoke(folder.Path.FullPath);
            var newFolder = folder.FileSystem.GetFolder(newFolderPath);
            await newFolder.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting);
            return newFolder;
        }
        
        private static StoragePath ToPath(StoragePath basePath, IEnumerable<string> pathSegments)
        {
            return pathSegments.Aggregate(basePath, (currentPath, segment) => currentPath / segment);
        }

    }

}
