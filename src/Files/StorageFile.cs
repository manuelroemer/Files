namespace Files
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files.Resources;

    /// <summary>
    ///     An immutable representation of a file in a file system.
    /// </summary>
    [DebuggerDisplay("StorageFile at {ToString()}")]
    public abstract class StorageFile : StorageElement
    {
        /// <summary>
        ///     Returns this file's parent folder.
        /// </summary>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance which represents the parent of this file.
        /// </returns>
        public StorageFolder GetParent()
        {
            var parentPath = Path.FullPath.Parent;

            // The Path API says that the parent path can be null and we should check for it.
            // Ideally, the file implementation guards against this when the file is created,
            // as realistically, every file must have a parent directory.
            // We are throwing here to notify library implementers.
            if (parentPath is null)
            {
                throw new InvalidOperationException(ExceptionStrings.File.HasNoParentPath());
            }

            return FileSystem.GetFolder(parentPath);
        }

        /// <summary>
        ///     Returns basic properties of the file.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public abstract Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Copies the file to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the file should be copied.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the newly created copy of this file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="destinationPath"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> has a type which is not compatible with this file's
        ///     file system implementation.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file or the destination folder is restricted.
        /// </exception>
        /// <exception cref="IOException">
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
        public Task<StorageFile> CopyAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            CopyAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Copies the file to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the file should be copied.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another element with a conflicting name already exists in the
        ///     destination folder.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the newly created copy of this file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="destinationPath"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> has a type which is not compatible with this file's
        ///     file system implementation.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file or the destination folder is restricted.
        /// </exception>
        /// <exception cref="IOException">
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
        public abstract Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Moves the file to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the file should be moved.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the new location of the moved file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="destinationPath"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> has a type which is not compatible with this file's
        ///     file system implementation.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file or the destination folder is restricted.
        /// </exception>
        /// <exception cref="IOException">
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
        public Task<StorageFile> MoveAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            MoveAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Moves the file to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the file should be moved.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another element with a conflicting name already exists in the
        ///     destination folder.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the new location of the moved file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="destinationPath"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> has a type which is not compatible with this file's
        ///     file system implementation.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file or the destination folder is restricted.
        /// </exception>
        /// <exception cref="IOException">
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
        public abstract Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Renames the file.
        /// </summary>
        /// <param name="newName">The new name of the file.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the new location of the renamed file.
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
        ///         <item>
        ///             <description>Any invalid file name character</description>
        ///         </item>
        ///     </list>
        ///     
        ///     You can use the <see cref="FileSystem.PathInformation"/> property of this file's
        ///     <see cref="StorageElement.FileSystem"/> to determine which characters are allowed.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public Task<StorageFile> RenameAsync(string newName, CancellationToken cancellationToken = default) =>
            RenameAsync(newName, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Renames the file.
        /// </summary>
        /// <param name="newName">The new name of the file.</param>
        /// <param name="options">
        ///     Defines how to react if another file with a conflicting name already exists in the
        ///     current folder.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the new location of the renamed file.
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
        ///         <item>
        ///             <description>Any invalid file name character</description>
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
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the file's parent folders does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The file does not exist.
        /// </exception>
        public abstract Task<StorageFile> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Opens and returns a stream which can be used for reading and writing bytes from and
        ///     to the file.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Stream"/> which can be used to read and write bytes from and to the file.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="OpenAsync(FileAccess, CancellationToken)"/> with the
        ///     <see cref="FileAccess.ReadWrite"/> parameter.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public Task<Stream> OpenAsync(CancellationToken cancellationToken = default) =>
            OpenAsync(FileAccess.ReadWrite, cancellationToken);

        /// <summary>
        ///     Opens and returns a stream which, depending on the specified <see cref="FileAccess"/> value,
        ///     can be used for reading and/or writing bytes from and to the file.
        /// </summary>
        /// <param name="fileAccess">
        ///     Defines whether the stream should be readable, writeable or both.
        ///     This value defines the minimum required capabilities.
        ///     This means that the returned stream may, for example, be both readable and
        ///     writeable, even if the value was only <see cref="FileAccess.Read"/>.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Stream"/> which, depending on the specified <see cref="FileAccess"/> value,
        ///     can be used to read and write bytes from and to the file.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public abstract Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Reads and returns the file's content as a byte array.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A byte array containing the file's content.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public abstract Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Reads and returns the file's content as a string.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A byte array containing the file's content.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public Task<string> ReadTextAsync(CancellationToken cancellationToken = default) =>
            ReadTextAsync(encoding: null, cancellationToken);

        /// <summary>
        ///     Reads and returns the file's content as a string.
        /// </summary>
        /// <param name="encoding">
        ///     The encoding to be applied to the file's content.
        ///     This can be <see langword="null"/>. If so, the underlying file system will use a
        ///     default encoding.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A byte array containing the file's content.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public abstract Task<string> ReadTextAsync(
            Encoding? encoding,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Writes the specified <paramref name="bytes"/> to the file.
        /// </summary>
        /// <param name="bytes">The bytes to be written to the file.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="bytes"/>
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public abstract Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Writes the specified <paramref name="text"/> to the file.
        /// </summary>
        /// <param name="text">The text to be written to the file.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="text"/>
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public Task WriteTextAsync(string text, CancellationToken cancellationToken = default) =>
            WriteTextAsync(text, encoding: null, cancellationToken);

        /// <summary>
        ///     Writes the specified <paramref name="text"/> to the file.
        /// </summary>
        /// <param name="text">The text to be written to the file.</param>
        /// <param name="encoding">
        ///     The encoding to be applied to the string.
        ///     This can be <see langword="null"/>. If so, the underlying file system will use a
        ///     default encoding.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="text"/>
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the file is restricted.
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
        public abstract Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        );
    }
}
