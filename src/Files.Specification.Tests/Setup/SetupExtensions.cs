namespace Files.Specification.Tests.Setup
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Files;
    using Files.Specification.Tests.Utilities;

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
            return folder.GetFolder(Default.FolderName).GetFolder(Default.FolderName);
        }
        
        /// <summary>
        ///     Returns a non-existing folder whose parent folder is also missing.
        /// </summary>
        public static StorageFile GetFileWithNonExistingParent(this StorageFolder folder)
        {
            return folder.GetFolder(Default.FolderName).GetFile(Default.FileName);
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
            var (srcParentFolder, dstParentFolder) = await folder.SetupFolderAsync(
                basePath => basePath / Default.SrcParentFolderName,
                basePath => basePath / Default.DstParentFolderName
            );
            var srcFile = await srcParentFolder.SetupFileAsync(basePath => basePath / Default.SrcFileName);
            return (srcParentFolder, dstParentFolder, srcFile);
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
            var (srcParentFolder, dstParentFolder, srcFolder) = await folder.SetupFolderAsync(
                basePath => basePath / Default.SrcParentFolderName,
                basePath => basePath / Default.DstParentFolderName,
                basePath => basePath / Default.SrcParentFolderName / Default.SrcFolderName
            );
            return (srcParentFolder, dstParentFolder, srcFolder);
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
            var srcParentFolder = await folder.SetupFolderAsync(basePath => basePath / Default.SrcParentFolderName);
            var srcFile = await srcParentFolder.SetupFileAsync(basePath => basePath / Default.SrcFileName);
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
            var (src, dst) = await folder.SetupFileAsync(
                basePath => basePath / Default.FileName,
                basePath => basePath / Default.ConflictingFileName
            );
            return (src, dst);
        }

        /// <summary>
        ///     Sets up a file and returns a folder which points to the same location as the created file.
        ///     This can be used for testing how APIs behave when an element of another type exists
        ///     at the same location.
        /// </summary>
        public static async Task<StorageFolder> SetupFileAndGetFolderAtSameLocation(this StorageFolder folder)
        {
            await folder.SetupFileAsync(basePath => basePath / Default.SharedFileFolderName);
            return folder.GetFolder(Default.SharedFileFolderName);
        }

        /// <summary>
        ///     Sets up a folder and returns a file which points to the same location as the created folder.
        ///     This can be used for testing how APIs behave when an element of another type exists
        ///     at the same location.
        /// </summary>
        public static async Task<StorageFile> SetupFolderAndGetFileAtSameLocation(this StorageFolder folder)
        {
            await folder.SetupFolderAsync(basePath => basePath / Default.SharedFileFolderName);
            return folder.GetFile(Default.SharedFileFolderName);
        }

        /// <summary>
        ///     Sets up and returns a file with the <see cref="Default.FileName"/> in the folder.
        /// </summary>
        public static Task<StorageFile> SetupFileAsync(this StorageFolder folder)
        {
            return folder.SetupFileAsync(basePath => basePath / Default.FileName);
        }

        /// <summary>
        ///     Sets up and returns a file which is located by the <paramref name="pathProvider"/>.
        ///     This also creates any non-existing parent folder.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which returns the path of the file to be created, based on the initial folder.
        /// </param>
        public static async Task<StorageFile> SetupFileAsync(this StorageFolder folder, PathProvider pathProvider)
        {
            var files = await folder.SetupFileAsync(new[] { pathProvider });
            return files[0];
        }

        /// <summary>
        ///     Sets up and returns multiple files which are located by the <paramref name="pathProviders"/>.
        ///     This also creates any non-existing parent folders.
        /// </summary>
        /// <param name="pathProviders">
        ///     Functions which return the path of the files to be created, based on the initial folder.
        /// </param>
        public static async Task<IList<StorageFile>> SetupFileAsync(this StorageFolder folder, params PathProvider[] pathProviders)
        {
            var files = pathProviders
                .Select(pathFactory => pathFactory(folder.Path))
                .Select(path => folder.FileSystem.GetFile(path))
                .ToList();

            foreach (var fileToCreate in files)
            {
                await fileToCreate.CreateAsync(recursive: true, options: CreationCollisionOption.Ignore);
            }

            return files;
        }

        /// <summary>
        ///     Sets up and returns a file with the <see cref="Default.FolderName"/> in the folder.
        /// </summary>
        public static Task<StorageFolder> SetupFolderAsync(this StorageFolder folder)
        {
            return folder.SetupFolderAsync(basePath => basePath / Default.FolderName);
        }

        /// <summary>
        ///     Sets up and returns a folder which is located by the <paramref name="pathProvider"/>.
        ///     This also creates any non-existing parent folder.
        /// </summary>
        /// <param name="pathProvider">
        ///     A function which returns the path of the folder to be created, based on the initial folder.
        /// </param>
        public static async Task<StorageFolder> SetupFolderAsync(this StorageFolder folder, PathProvider pathProvider)
        {
            var folders = await folder.SetupFolderAsync(new[] { pathProvider });
            return folders[0];
        }

        /// <summary>
        ///     Sets up and returns multiple folders which are located by the <paramref name="pathProviders"/>.
        ///     This also creates any non-existing parent folders.
        /// </summary>
        /// <param name="pathProviders">
        ///     Functions which return the path of the folders to be created, based on the initial folder.
        /// </param>
        public static async Task<IList<StorageFolder>> SetupFolderAsync(this StorageFolder folder, params PathProvider[] pathProviders)
        {
            var folders = pathProviders
                .Select(pathFactory => pathFactory(folder.Path))
                .Select(path => folder.FileSystem.GetFolder(path))
                .ToList();

            foreach (var folderToCreate in folders)
            {
                await folderToCreate.CreateAsync(recursive: true, options: CreationCollisionOption.Ignore);
            }

            return folders;
        }

    }

}
