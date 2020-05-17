namespace Files
{
    using System;
    using System.Threading.Tasks;
    using System.IO;
    using System.Threading;

    /// <summary>
    ///     Defines extension methods for the <see cref="StorageFile"/> class.
    /// </summary>
    public static class StorageFileExtensions
    {
        /// <summary>
        ///     Copies the file to the specified location.
        ///     Any existing file at the destination will be replaced.
        /// </summary>
        /// <param name="storageFile">The <see cref="StorageFile"/>.</param>
        /// <param name="destinationPath">
        ///     The location to which the file should be copied.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the newly created copy of this file.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFile.CopyAsync(StoragePath, NameCollisionOption, CancellationToken)"/>
        ///     with the <see cref="NameCollisionOption.ReplaceExisting"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="storageFile"/> or <paramref name="destinationPath"/> is <see langword="null"/>.
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
        ///     Access to the file or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the file's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     A conflicting folder exists at the destination.
        ///     
        ///     -or-
        ///     
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the file's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The destination folder does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file does not exist.
        /// </exception>
        public static Task<StorageFile> CopyOrReplaceExistingAsync(
            this StorageFile storageFile,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.CopyAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Moves the file to the specified location.
        ///     Any existing file at the destination will be replaced.
        /// </summary>
        /// <param name="storageFile">The <see cref="StorageFile"/>.</param>
        /// <param name="destinationPath">
        ///     The location to which the file should be moved.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the new location of the moved file.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFile.MoveAsync(StoragePath, NameCollisionOption, CancellationToken)"/>
        ///     with the <see cref="NameCollisionOption.ReplaceExisting"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="storageFile"/> or <paramref name="destinationPath"/> is <see langword="null"/>.
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
        ///     Access to the file or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the file's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     A conflicting folder exists at the destination.
        ///     
        ///     -or-
        ///     
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the file's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The destination folder does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file does not exist.
        /// </exception>
        public static Task<StorageFile> MoveOrReplaceExistingAsync(
            this StorageFile storageFile,
            StoragePath destinationPath,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.MoveAsync(destinationPath, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Renames the file.
        ///     Any existing file at the destination will be replaced.
        /// </summary>
        /// <param name="storageFile">The <see cref="StorageFile"/>.</param>
        /// <param name="newName">The new name of the file.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the renamed file.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFile.RenameAsync(string, NameCollisionOption, CancellationToken)"/>
        ///     with the <see cref="NameCollisionOption.ReplaceExisting"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="storageFile"/> or <paramref name="newName"/> is <see langword="null"/>.
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
        ///     You can use the <see cref="FileSystem.PathInformation"/> property of this file's
        ///     <see cref="StorageElement.FileSystem"/> property to determine which characters are allowed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the file's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     A conflicting folder exists at the destination.
        ///     
        ///     -or-
        ///     
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the file's parent folders does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file does not exist.
        /// </exception>
        public static Task<StorageFile> RenameOrReplaceExistingAsync(
            this StorageFile storageFile,
            string newName,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.RenameAsync(newName, NameCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Opens and returns a stream which can be used for reading bytes from the file.
        /// </summary>
        /// <param name="storageFile">The <see cref="StorageFile"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Stream"/> which can be used to read bytes from the file.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFile.OpenAsync(FileAccess, CancellationToken)"/> with the
        ///     <see cref="FileAccess.Read"/> parameter.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the file's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the file's parent folders does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file does not exist.
        /// </exception>
        public static Task<Stream> OpenReadAsync(
            this StorageFile storageFile,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.OpenAsync(FileAccess.Read, cancellationToken);
        }

        /// <summary>
        ///     Opens and returns a stream which can be used for writing bytes to the file.
        /// </summary>
        /// <param name="storageFile">The <see cref="StorageFile"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Stream"/> which can be used to write bytes to the file.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageFile.OpenAsync(FileAccess, CancellationToken)"/> with the
        ///     <see cref="FileAccess.Write"/> parameter.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the file's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the file's parent folders does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file does not exist.
        /// </exception>
        public static Task<Stream> OpenWriteAsync(
            this StorageFile storageFile,
            CancellationToken cancellationToken = default
        )
        {
            _ = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
            return storageFile.OpenAsync(FileAccess.Write, cancellationToken);
        }
    }
}
