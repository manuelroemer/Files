namespace Files
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An immutable representation of a folder in a file system.
    /// </summary>
    public abstract class Folder : FileSystemElement
    {

        /// <summary>
        ///     Returns this folder's parent folder or <see langword="null"/> if it is a root folder.
        /// </summary>
        /// <returns>
        ///     A <see cref="Folder"/> instance which represents the parent of this folder or
        ///     <see langword="null"/> if this folder cannot possibly have a parent folder, i.e.
        ///     if it is a root folder.
        /// </returns>
        public virtual Folder? GetParent()
        {
            var parentPath = Path.FullPath.Parent;
            return parentPath is null
                ? null
                : FileSystem.GetFolder(parentPath);
        }

        /// <summary>
        ///     Returns a file relative to this folder by joining this folder's full path with specified
        ///     <paramref name="name"/> and returning a new <see cref="File"/> instance created from
        ///     the resulting path.
        /// </summary>
        /// <param name="name">
        ///     The name of the file.
        /// </param>
        /// <returns>
        ///     A new <see cref="File"/> instance which represents a file with the specified
        ///     <paramref name="name"/> relative to this folder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public virtual File GetFile(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            var path = Path.FullPath.Join(name);
            return FileSystem.GetFile(path);
        }

        /// <summary>
        ///     Returns a folder relative to this folder by joining this folder's full path with specified
        ///     <paramref name="name"/> and returning a new <see cref="Folder"/> instance created from
        ///     the resulting path.
        /// </summary>
        /// <param name="name">
        ///     The name of the folder.
        /// </param>
        /// <returns>
        ///     A new <see cref="Folder"/> instance which represents a folder with the specified
        ///     <paramref name="name"/> relative to this folder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public virtual Folder GetFolder(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            var path = Path.FullPath.Join(name);
            return FileSystem.GetFolder(path);
        }

        /// <summary>
        ///     Returns basic properties of the folder.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the folder is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the folder's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The folder does not exist.
        /// </exception>
        public abstract Task<FolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default);

        public Task<Folder> CopyAsync(Path destinationPath, CancellationToken cancellationToken = default) =>
            CopyAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        public abstract Task<Folder> CopyAsync(
            Path destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        public Task<Folder> MoveAsync(Path destinationPath, CancellationToken cancellationToken = default) =>
            MoveAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        public abstract Task<Folder> MoveAsync(
            Path destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        public Task<Folder> RenameAsync(string newName, CancellationToken cancellationToken = default) =>
            RenameAsync(newName, DefaultNameCollisionOption, cancellationToken);

        public abstract Task<Folder> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        public virtual async Task<IEnumerable<FileSystemElement>> GetAllChildrenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var getFilesTask = GetAllFilesAsync(cancellationToken);
            var getFoldersTask = GetAllFoldersAsync(cancellationToken);
            
            var files = await getFilesTask.ConfigureAwait(false);
            var folders = await getFoldersTask.ConfigureAwait(false);

            return Enumerable.Concat<FileSystemElement>(files, folders);
        }

        public abstract Task<IEnumerable<File>> GetAllFilesAsync(CancellationToken cancellationToken = default);

        public abstract Task<IEnumerable<Folder>> GetAllFoldersAsync(CancellationToken cancellationToken = default);

    }

}
