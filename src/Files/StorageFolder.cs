namespace Files
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An immutable representation of a folder in a file system.
    /// </summary>
    [DebuggerDisplay("📁 {ToString()}")]
    public abstract class StorageFolder : StorageElement
    {
        private readonly Lazy<StorageFolder?> _parentLazy;

        /// <summary>
        ///     Gets this folder's parent folder or <see langword="null"/> if it is a root folder.
        /// </summary>
        public StorageFolder? Parent => _parentLazy.Value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageFolder"/> class.
        /// </summary>
        public StorageFolder()
        {
            _parentLazy = new Lazy<StorageFolder?>(() =>
            {
                var parentPath = Path.FullPath.Parent;
                return parentPath is null
                    ? null
                    : FileSystem.GetFolder(parentPath);
            });
        }

        /// <inheritdoc/>
        public sealed override StorageFile AsFile() =>
            FileSystem.GetFile(Path);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public sealed override StorageFolder AsFolder() =>
            this;

        /// <summary>
        ///     Returns a file relative to this folder by joining this folder's full path with the specified
        ///     <paramref name="name"/> and returning a new <see cref="StorageFile"/> instance created from
        ///     the resulting path.
        /// </summary>
        /// <param name="name">
        ///     The name of the file.
        /// </param>
        /// <returns>
        ///     A new <see cref="StorageFile"/> instance which represents a file with the specified
        ///     <paramref name="name"/> relative to this folder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public StorageFile GetFile(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            var path = Path.FullPath.Join(name);
            return FileSystem.GetFile(path);
        }

        /// <summary>
        ///     Returns a folder relative to this folder by joining this folder's full path with the specified
        ///     <paramref name="name"/> and returning a new <see cref="StorageFolder"/> instance created from
        ///     the resulting path.
        /// </summary>
        /// <param name="name">
        ///     The name of the folder.
        /// </param>
        /// <returns>
        ///     A new <see cref="StorageFolder"/> instance which represents a folder with the specified
        ///     <paramref name="name"/> relative to this folder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public StorageFolder GetFolder(string name)
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
        public abstract Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default);

        public Task<StorageFolder> CopyAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            CopyAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        public abstract Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        public Task<StorageFolder> MoveAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            MoveAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        public abstract Task<StorageFolder> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Renames the folder.
        /// </summary>
        /// <param name="newName">The new name of the folder.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the new location of the renamed folder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="newName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="newName"/> is empty or contains one or more invalid characters.
        ///     Invalid characters are:
        ///     
        ///     <list type="bullet">
        ///         <item>
        ///             <description>The (alt) directory separator character</description>
        ///         </item>
        ///         <item>
        ///             <description>The volume separator character</description>
        ///         </item>
        ///         <item>
        ///             <description>Any invalid path character</description>
        ///         </item>
        ///     </list>
        ///     
        ///     You can use the <see cref="FileSystem.PathInformation"/> property of this folder's
        ///     <see cref="StorageElement.FileSystem"/> property to determine which characters are allowed.
        /// </exception>
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
        public Task<StorageFolder> RenameAsync(string newName, CancellationToken cancellationToken = default) =>
            RenameAsync(newName, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Renames the folder.
        /// </summary>
        /// <param name="newName">The new name of the folder.</param>
        /// <param name="options">
        ///     Defines how to react if another folder with a conflicting name already exists in the
        ///     current folder.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the new location of the renamed folder.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="newName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="newName"/> is empty or contains one or more invalid characters.
        ///     Invalid characters are:
        ///     
        ///     <list type="bullet">
        ///         <item>
        ///             <description>The (alt) directory separator character</description>
        ///         </item>
        ///         <item>
        ///             <description>The volume separator character</description>
        ///         </item>
        ///         <item>
        ///             <description>Any invalid path character</description>
        ///         </item>
        ///     </list>
        ///     
        ///     You can use the <see cref="FileSystem.PathInformation"/> property of this folder's
        ///     <see cref="StorageElement.FileSystem"/> property to determine which characters are allowed.
        /// </exception>
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
        public abstract Task<StorageFolder> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        public virtual async Task<IEnumerable<StorageElement>> GetAllChildrenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var getFilesTask = GetAllFilesAsync(cancellationToken);
            var getFoldersTask = GetAllFoldersAsync(cancellationToken);
            
            var files = await getFilesTask.ConfigureAwait(false);
            var folders = await getFoldersTask.ConfigureAwait(false);

            return Enumerable.Concat<StorageElement>(files, folders);
        }

        public abstract Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default);

        public abstract Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default);
    }
}
