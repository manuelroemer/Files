namespace Files
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Defines extension methods for the <see cref="StorageFolder"/> class.
    /// </summary>
    public static class StorageFolderExtensions
    {
        /// <summary>
        ///     Copies the folder to the specified location.
        ///     Any existing folder at the destination will be replaced.
        /// </summary>
        /// <param name="storageFolder">The <see cref="StorageFolder"/>.</param>
        /// <param name="destinationPath">
        ///     The location to which the folder should be copied.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the newly created copy of this folder.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFolder.CopyAsync(StoragePath, NameCollisionOption, CancellationToken)"/>
        ///     with the <see cref="NameCollisionOption.ReplaceExisting"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="storageFolder"/> or <paramref name="destinationPath"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> is a <see cref="StoragePath"/> instance representing a path
        ///     which is considered invalid by this file system implementation.
        ///     This can occur if you are using multiple <see cref="FileSystem"/> implementations
        ///     simultaneously.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the folder or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     A conflicting file exists at the destination.
        ///     
        ///     -or-
        ///     
        ///     An I/O error occured while interacting with the folder system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The destination folder does not exist.
        /// </exception>
        public static Task<StorageFolder> CopyOrReplaceExistingAsync(
            this StorageFolder storageFolder,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));
            return storageFolder.CopyAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Moves the folder to the specified location.
        ///     Any existing folder at the destination will be replaced.
        /// </summary>
        /// <param name="storageFolder">The <see cref="StorageFolder"/>.</param>
        /// <param name="destinationPath">
        ///     The location to which the folder should be moved.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the new location of the moved folder.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFolder.MoveAsync(StoragePath, NameCollisionOption, CancellationToken)"/>
        ///     with the <see cref="NameCollisionOption.ReplaceExisting"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="storageFolder"/> or <paramref name="destinationPath"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> is a <see cref="StoragePath"/> instance representing a path
        ///     which is considered invalid by this file system implementation.
        ///     This can occur if you are using multiple <see cref="FileSystem"/> implementations
        ///     simultaneously.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the folder or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     A conflicting file exists at the destination.
        ///     
        ///     -or-
        ///     
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The destination folder does not exist.
        /// </exception>
        public static Task<StorageFolder> MoveOrReplaceExistingAsync(
            this StorageFolder storageFolder,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));
            return storageFolder.MoveAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Renames the folder.
        ///     Any existing folder at the destination will be replaced.
        /// </summary>
        /// <param name="storageFolder">The <see cref="StorageFolder"/>.</param>
        /// <param name="newName">The new name of the folder.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the renamed folder.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFolder.RenameAsync(string, NameCollisionOption, CancellationToken)"/>
        ///     with the <see cref="NameCollisionOption.ReplaceExisting"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="storageFolder"/> or <paramref name="newName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="newName"/> is empty or contains one or more invalid characters.
        ///     Invalid characters are:
        ///     
        ///     <list type="bullet">
        ///         <item>
        ///             <description>The directory separator character</description>
        ///         </item>
        ///         <item>
        ///             <description>The alternative directory separator character</description>
        ///         </item>
        ///         <item>
        ///             <description>The volume separator character</description>
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
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     A conflicting file exists at the destination.
        ///     
        ///     -or-
        ///     
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public static Task<StorageFolder> RenameOrReplaceExistingAsync(
            this StorageFolder storageFolder,
            string newName,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFolder ?? throw new ArgumentNullException(nameof(storageFolder));
            return storageFolder.RenameAsync(newName, NameCollisionOption.ReplaceExisting, cancellationToken);
        }
    }
}
