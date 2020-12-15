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
    ///     Instances can be created via the <see cref="FileSystem"/> class.
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
        /// <param name="fileSystem">
        ///     The file system with which this <see cref="StorageElement"/> is associated.
        /// </param>
        /// <param name="path">
        ///     The path which locates this element.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="fileSystem"/> or <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        protected StorageFolder(FileSystem fileSystem, StoragePath path)
            : base(fileSystem, path)
        {
            _parentLazy = new Lazy<StorageFolder?>(() =>
            {
                var parentPath = Path.FullPath.Parent;
                return parentPath is null
                    ? null
                    : FileSystem.GetFolder(parentPath);
            });
        }

        /// <summary>
        ///     Returns a new <see cref="StorageFile"/> with the same path as this
        ///     <see cref="StorageFolder"/> instance.
        /// </summary>
        /// <returns>
        ///     A new <see cref="StorageFile"/> with the same path as this <see cref="StorageFolder"/> instance.
        /// </returns>
        /// <remarks>
        ///     Using this method is equivalent to calling <c>element.FileSystem.GetFile(folder.Path)</c>.
        /// </remarks>
        public sealed override StorageFile AsFile() =>
            FileSystem.GetFile(Path);

        /// <summary>
        ///     Returns the same <see cref="StorageFolder"/> instance.
        /// </summary>
        /// <returns>The same <see cref="StorageFolder"/> instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public sealed override StorageFolder AsFolder() =>
            this;

        /// <summary>
        ///     Returns a file relative to this folder by linking this folder's full path with the specified
        ///     <paramref name="name"/> and returning a new <see cref="StorageFile"/> instance created from
        ///     the resulting path.
        /// </summary>
        /// <param name="name">
        ///     The name of the file.
        ///     This can both be an empty string and a fully fledged path (even though this is not recommended -
        ///     concatenating the paths manually makes your intent clearer).
        ///     
        ///     This name is appended to the folder's full path via the <see cref="StoragePath.Link(string)"/> method.
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
            var path = Path.FullPath.Link(name);
            return FileSystem.GetFile(path);
        }

        /// <summary>
        ///     Returns a folder relative to this folder by linking this folder's full path with the specified
        ///     <paramref name="name"/> and returning a new <see cref="StorageFolder"/> instance created from
        ///     the resulting path.
        /// </summary>
        /// <param name="name">
        ///     The name of the folder.
        ///     This can both be an empty string and a fully fledged path (even though this is not recommended -
        ///     concatenating the paths manually makes your intent clearer).
        ///     
        ///     This name is appended to the folder's full path via the <see cref="StoragePath.Link(string)"/> method.
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
            var path = Path.FullPath.Link(name);
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
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     The folder does not exist.
        /// 
        ///     -or-
        /// 
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public abstract Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Copies the folder and all of its contents to the specified location.
        ///     An exception is thrown if a folder already exists at the destination.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the folder should be copied.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the newly created copy of this folder.
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
        ///     Access to the folder or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     Another folder already exists at the destination.
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
        public Task<StorageFolder> CopyAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            CopyAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Copies the folder and all of its contents to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the folder should be copied.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another folder with a conflicting name already exists at the destination.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the newly created copy of this folder.
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
        ///     Access to the folder or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     Another folder already exists at the destination and <paramref name="options"/> has
        ///     the value <see cref="CreationCollisionOption.Fail"/>.
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
        public abstract Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Moves the folder and all of its contents to the specified location.
        ///     An exception is thrown if a folder already exists at the destination.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the folder should be moved.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the new location of the moved folder.
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
        ///     Access to the folder or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     Another folder already exists at the destination.
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
        public Task<StorageFolder> MoveAsync(StoragePath destinationPath, CancellationToken cancellationToken = default) =>
            MoveAsync(destinationPath, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Moves the folder and all of its contents to the specified location.
        /// </summary>
        /// <param name="destinationPath">
        ///     The location to which the folder should be moved.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another folder with a conflicting name already exists at the destination.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the new location of the moved folder.
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
        ///     Access to the folder or the destination folder is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of the folder's path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     Another folder already exists at the destination and <paramref name="options"/> has
        ///     the value <see cref="CreationCollisionOption.Fail"/>.
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
        public abstract Task<StorageFolder> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Renames the folder.
        ///     An exception is thrown if a folder already exists at the rename destination.
        /// </summary>
        /// <param name="newName">The new name of the folder.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the renamed folder.
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
        ///     You can use the <see cref="FileSystem.PathInformation"/> property of this folder's
        ///     <see cref="StorageElement.FileSystem"/> to determine which characters are allowed.
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
        ///     Another folder already exists at the destination.
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
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public Task<StorageFolder> RenameAsync(string newName, CancellationToken cancellationToken = default) =>
            RenameAsync(newName, DefaultNameCollisionOption, cancellationToken);

        /// <summary>
        ///     Renames the folder.
        /// </summary>
        /// <param name="newName">The new name of the folder.</param>
        /// <param name="options">
        ///     Defines how to react if another folder with a conflicting name already exists at the destination.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="StorageFolder"/> instance representing the renamed folder.
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
        ///     Another folder already exists at the destination and <paramref name="options"/> has
        ///     the value <see cref="CreationCollisionOption.Fail"/>.
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
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public abstract Task<StorageFolder> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Returns all top-level children (both files and folders) contained by the folder.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A set of <see cref="StorageElement"/> instances which, at the time of calling this method,
        ///     were child elements of the folder.
        /// </returns>
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
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public virtual async Task<IEnumerable<StorageElement>> GetAllChildrenAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var getFilesTask = GetAllFilesAsync(cancellationToken);
            var getFoldersTask = GetAllFoldersAsync(cancellationToken);
            
            var files = await getFilesTask.ConfigureAwait(false);
            var folders = await getFoldersTask.ConfigureAwait(false);

            return Enumerable.Concat<StorageElement>(files, folders);
        }

        /// <summary>
        ///     Returns all top-level files contained by the folder.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A set of <see cref="StorageFile"/> instances which, at the time of calling this method,
        ///     were child elements of the folder.
        /// </returns>
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
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public abstract Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns all top-level folders contained by the folder.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A set of <see cref="StorageFolder"/> instances which, at the time of calling this method,
        ///     were child elements of the folder.
        /// </returns>
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
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     The folder does not exist.
        ///     
        ///     -or-
        ///     
        ///     One of the folder's parent folders does not exist.
        /// </exception>
        public abstract Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default);
    }
}
