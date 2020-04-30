namespace Files
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files.Shared;

    /// <summary>
    ///     An immutable representation of a file in a file system.
    ///     Instances can be created via the <see cref="FileSystem"/> class.
    /// </summary>
    [DebuggerDisplay("📄 {ToString()}")]
    public abstract class StorageFile : StorageElement
    {
        private readonly Lazy<StorageFolder> _parentLazy;

        /// <summary>
        ///     Gets this file's parent folder.
        /// </summary>
        public StorageFolder Parent => _parentLazy.Value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageFile"/> class.
        /// </summary>
        public StorageFile()
        {
            _parentLazy = new Lazy<StorageFolder>(() =>
            {
                var parentPath = Path.FullPath.Parent;

                // The Path API declares that the parent path can be null and we should check for it.
                // The specification and assumptions of this library assume that each file has a
                // parent folder though. Therefore this exception is undocumented, because it is
                // treated as an implementation error and should never occur.
                if (parentPath is null)
                {
                    throw new InvalidOperationException(ExceptionStrings.StorageFile.HasNoParentPath());
                }

                return FileSystem.GetFolder(parentPath);
            });
        }

        /// <summary>
        ///     Returns the same <see cref="StorageFile"/> instance.
        /// </summary>
        /// <returns>The same <see cref="StorageFile"/> instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public sealed override StorageFile AsFile() =>
            this;

        /// <summary>
        ///     Returns a new <see cref="StorageFolder"/> with the same path as this
        ///     <see cref="StorageFile"/> instance.
        /// </summary>
        /// <returns>
        ///     A new <see cref="StorageFolder"/> with the same path as this <see cref="StorageFile"/> instance.
        /// </returns>
        /// <remarks>
        ///     Using this method is equivalent to calling <c>element.FileSystem.GetFolder(file.Path)</c>.
        /// </remarks>
        public sealed override StorageFolder AsFolder() =>
            FileSystem.GetFolder(Path);

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
        public abstract Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Copies the file to the specified location.
        ///     An exception is thrown if a file exists at the destination.
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
        ///     <paramref name="destinationPath"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> is a <see cref="StoragePath"/> instance which is incompatible
        ///     with this <see cref="FileSystem"/> implementation.
        ///     This exception generally occurs when you are using multiple <see cref="FileSystem"/>
        ///     implementations simultaneously.
        /// 
        ///     This exception is <b>always</b> thrown when the type of <paramref name="destinationPath"/>
        ///     doesn't match the specific <see cref="StoragePath"/> type created by the current
        ///     <see cref="FileSystem"/> implementation.
        ///     This condition <b>may</b>, however, be enhanced by any <see cref="FileSystem"/>
        ///     implementation.
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
        ///     Another file already exists at the destination.
        ///     
        ///     -or-
        ///     
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
        public Task<StorageFile> CopyAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            CopyAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Copies the file to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the file should be copied.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another file with a conflicting name already exists at the destination.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the newly created copy of this file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="destinationPath"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> is a <see cref="StoragePath"/> instance which is incompatible
        ///     with this <see cref="FileSystem"/> implementation.
        ///     This exception generally occurs when you are using multiple <see cref="FileSystem"/>
        ///     implementations simultaneously.
        /// 
        ///     This exception is <b>always</b> thrown when the type of <paramref name="destinationPath"/>
        ///     doesn't match the specific <see cref="StoragePath"/> type created by the current
        ///     <see cref="FileSystem"/> implementation.
        ///     This condition <b>may</b>, however, be enhanced by any <see cref="FileSystem"/>
        ///     implementation.
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
        ///     Another file already exists at the destination and <paramref name="options"/> has
        ///     the value <see cref="CreationCollisionOption.Fail"/>.
        ///     
        ///     -or-
        ///     
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
        public abstract Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Moves the file to the specified location.
        ///     An exception is thrown if a file already exists at the destination.
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
        ///     <paramref name="destinationPath"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> is a <see cref="StoragePath"/> instance which is incompatible
        ///     with this <see cref="FileSystem"/> implementation.
        ///     This exception generally occurs when you are using multiple <see cref="FileSystem"/>
        ///     implementations simultaneously.
        /// 
        ///     This exception is <b>always</b> thrown when the type of <paramref name="destinationPath"/>
        ///     doesn't match the specific <see cref="StoragePath"/> type created by the current
        ///     <see cref="FileSystem"/> implementation.
        ///     This condition <b>may</b>, however, be enhanced by any <see cref="FileSystem"/>
        ///     implementation.
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
        ///     Another file already exists at the destination.
        ///     
        ///     -or-
        ///     
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
        public Task<StorageFile> MoveAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            MoveAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Moves the file to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the file should be moved.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another file with a conflicting name already exists at the destination.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the new location of the moved file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="destinationPath"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="destinationPath"/> is a <see cref="StoragePath"/> instance which is incompatible
        ///     with this <see cref="FileSystem"/> implementation.
        ///     This exception generally occurs when you are using multiple <see cref="FileSystem"/>
        ///     implementations simultaneously.
        /// 
        ///     This exception is <b>always</b> thrown when the type of <paramref name="destinationPath"/>
        ///     doesn't match the specific <see cref="StoragePath"/> type created by the current
        ///     <see cref="FileSystem"/> implementation.
        ///     This condition <b>may</b>, however, be enhanced by any <see cref="FileSystem"/>
        ///     implementation.
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
        ///     Another file already exists at the destination and <paramref name="options"/> has
        ///     the value <see cref="CreationCollisionOption.Fail"/>.
        ///     
        ///     -or-
        ///     
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
        public abstract Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Renames the file.
        ///     An exception is thrown if a file already exists at the rename destination.
        /// </summary>
        /// <param name="newName">The new name of the file.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the renamed file.
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
        ///     <see cref="StorageElement.FileSystem"/> to determine which characters are allowed.
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
        ///     Another file already exists at the destination.
        ///     
        ///     -or-
        ///     
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
        public Task<StorageFile> RenameAsync(string newName, CancellationToken cancellationToken = default) =>
            RenameAsync(newName, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Renames the file.
        /// </summary>
        /// <param name="newName">The new name of the file.</param>
        /// <param name="options">
        ///     Defines how to react if another file with a conflicting name already exists at the destination.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFile"/> instance representing the renamed file.
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
        ///     Another file already exists at the destination and <paramref name="options"/> has
        ///     the value <see cref="CreationCollisionOption.Fail"/>.
        ///     
        ///     -or-
        ///     
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
        ///     <paramref name="bytes"/> is <see langword="null"/>.
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
        ///     <paramref name="text"/> is <see langword="null"/>.
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
        ///     <paramref name="text"/> is <see langword="null"/>.
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
        public abstract Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        );
    }
}
