namespace Files.Specification.Tests.Preparation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Files;
    using Files.Specification.Tests.Preparation;
    using Files.Specification.Tests.Utilities;

    /// <summary>
    ///     Defines static extension methods for conveniently setting up common file system structures
    ///     which are frequently used in tests.
    ///     These structures use the names defined in <see cref="Default"/>.
    /// </summary>
    public static class SetupExtensions
    {

        /// <summary>
        ///     Sets up a default folder structure for tests which require a source and destination file using the <see cref="Default.SrcFolderName"/>,
        ///     <see cref="Default.DstFolderName"/> and <see cref="Default.SrcFileName"/> names.
        ///     
        ///     The final structure looks similar to this:
        ///     
        ///     <code>
        ///     folder
        ///     |_ src
        ///     |  |_ src.file
        ///     |_ dst
        ///     </code>
        /// </summary>
        public static async Task<(Folder SrcFolder, Folder DstFolder, File SrcFile)> SetupSrcDstFileAsync(this Folder folder)
        {
            var (src, dst) = await folder.SetupFolderAsync(
                basePath => basePath / Default.SrcFolderName,
                basePath => basePath / Default.DstFolderName
            ).ConfigureAwait(false);
            var srcFile = await src.SetupFileAsync(basePath => basePath / Default.SrcFileName);

            return (src, dst, srcFile);
        }

        /// <summary>
        ///     Sets up two files which can be used for testing file conflicts using the <see cref="Default.FileName"/>
        ///     and <see cref="Default.ConflictingFileName"/> names.
        ///     
        ///     The final structure looks similar to this:
        ///     
        ///     <code>
        ///     folder
        ///     |_ src.file
        ///     |_ dst.file
        ///     </code>
        /// </summary>
        public static async Task<(File SrcFile, File ConflictingDstFile)> SetupTwoConflictingFilesAsync(this Folder folder)
        {
            var (src, dst) = await folder.SetupFileAsync(
                basePath => basePath / Default.FileName,
                basePath => basePath / Default.ConflictingFileName
            ).ConfigureAwait(false);
            return (src, dst);
        }

        /// <summary>
        ///     Sets up and returns a file with the <see cref="Default.FileName"/> in the folder.
        /// </summary>
        public static Task<File> SetupFileAsync(this Folder folder)
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
        public static async Task<File> SetupFileAsync(this Folder folder, PathProvider pathProvider)
        {
            var files = await folder.SetupFileAsync(new[] { pathProvider }).ConfigureAwait(false);
            return files[0];
        }

        /// <summary>
        ///     Sets up and returns multiple files which are located by the <paramref name="pathProviders"/>.
        ///     This also creates any non-existing parent folders.
        /// </summary>
        /// <param name="pathProviders">
        ///     Functions which return the path of the files to be created, based on the initial folder.
        /// </param>
        public static async Task<IList<File>> SetupFileAsync(this Folder folder, params PathProvider[] pathProviders)
        {
            var files = pathProviders
                .Select(pathFactory => pathFactory(folder.Path))
                .Select(path => folder.FileSystem.GetFile(path))
                .ToList();

            foreach (var fileToCreate in files)
            {
                await fileToCreate.CreateAsync(recursive: true, options: CreationCollisionOption.Ignore).ConfigureAwait(false);
            }

            return files;
        }

        /// <summary>
        ///     Sets up and returns a file with the <see cref="Default.FolderName"/> in the folder.
        /// </summary>
        public static Task<Folder> SetupFolderAsync(this Folder folder)
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
        public static async Task<Folder> SetupFolderAsync(this Folder folder, PathProvider pathProvider)
        {
            var folders = await folder.SetupFolderAsync(new[] { pathProvider }).ConfigureAwait(false);
            return folders[0];
        }

        /// <summary>
        ///     Sets up and returns multiple folders which are located by the <paramref name="pathProviders"/>.
        ///     This also creates any non-existing parent folders.
        /// </summary>
        /// <param name="pathProviders">
        ///     Functions which return the path of the folders to be created, based on the initial folder.
        /// </param>
        public static async Task<IList<Folder>> SetupFolderAsync(this Folder folder, params PathProvider[] pathProviders)
        {
            var folders = pathProviders
                .Select(pathFactory => pathFactory(folder.Path))
                .Select(path => folder.FileSystem.GetFolder(path))
                .ToList();

            foreach (var folderToCreate in folders)
            {
                await folderToCreate.CreateAsync(recursive: true, options: CreationCollisionOption.Ignore).ConfigureAwait(false);
            }

            return folders;
        }

    }

}
