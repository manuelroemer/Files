namespace Files
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An immutable representation of a file or folder in a file system.
    ///     This is the base class of the <see cref="StorageFile"/> and <see cref="StorageFolder"/> classes.
    ///     No other classes can derive from this class.
    /// </summary>
    /// <seealso cref="StorageFile"/>
    /// <seealso cref="StorageFolder"/>
    public abstract class StorageElement : IFileSystemElement
    {

        internal const CreationCollisionOption DefaultCreationCollisionOption = CreationCollisionOption.Fail;
        internal const NameCollisionOption DefaultNameCollisionOption = NameCollisionOption.Fail;
        internal const DeletionOption DefaultDeletionOption = DeletionOption.Fail;

        /// <inheritdoc/>
        public abstract FileSystem FileSystem { get; }

        /// <summary>
        ///     Gets the path which identifies this element.
        /// </summary>
        public abstract StoragePath Path { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageElement"/> class.
        /// </summary>
        /// <remarks>
        ///     This internal constructor ensures that only the <see cref="StorageFile"/> and
        ///     <see cref="StorageFolder"/> classes can derive from this base class.
        ///     External users of the library cannot derive from it.
        /// </remarks>
        private protected StorageElement()
        {
            // Never make this public or protected!
            // Only the file/folder classes within this library are supposed to inherit from this base.
        }

        /// <summary>
        ///     Gets the element's current attributes.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     A <see cref="FileAttributes"/> value which lists all current attributes of the file or folder.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The element is a <see cref="StorageFolder"/> and does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The element is a <see cref="StorageFile"/> and does not exist.
        /// </exception>
        public abstract Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Sets the element's attributes to all valid and supported values specified by the
        ///     <paramref name="attributes"/> parameter.
        ///     
        ///     This method does not throw an exception if the specified <paramref name="attributes"/>
        ///     value defines invalid or unsupported attributes or attribute combinations.
        ///     These values or combinations are ignored instead.
        /// </summary>
        /// <param name="attributes">
        ///     The new attributes for the file or folder.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The element is a <see cref="StorageFolder"/> and does not exist.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The element is a <see cref="StorageFile"/> and does not exist.
        /// </exception>
        public abstract Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns a value indicating whether the element physically exists in the file system
        ///     at this point in time.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the element physically exists in the file system at this
        ///     point in time;
        ///     <see langword="false"/> if not.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        /// </exception>
        public abstract Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates the element if it does not already exist.
        ///     An exception is thrown if the element already exists or if one of the element's parent
        ///     folders does not exist.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="CreateAsync(bool, CreationCollisionOption, CancellationToken)"/> with the
        ///     <see cref="CreationCollisionOption.Fail"/> and <c>recursive: false</c> parameters.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        ///     
        ///     -or-
        ///     
        ///     The element already exists.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        /// </exception>
        public Task CreateAsync(CancellationToken cancellationToken = default) =>
            CreateAsync(recursive: false, DefaultCreationCollisionOption, cancellationToken);

        /// <summary>
        ///     Creates the element and optionally all of its non-existing parent folders if it does not already exist.
        ///     An exception is thrown if the element already exists.
        /// </summary>
        /// <param name="recursive">
        ///     A value indicating whether the element's parent folder should be created recursively
        ///     if they don't already exist.
        ///     
        ///     If <see langword="true"/>, every parent folder which does not exist will be created
        ///     until a parent folder which already exists is reached.
        ///     
        ///     If <see langword="false"/>, this method will throw a <see cref="DirectoryNotFoundException"/>
        ///     if the element's parent folder does not exist.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="CreateAsync(bool, CreationCollisionOption, CancellationToken)"/> with the
        ///     <see cref="CreationCollisionOption.Fail"/> parameter.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        ///     
        ///     -or-
        ///     
        ///     The element already exists.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist and <paramref name="recursive"/>
        ///     is <see langword="false"/>.
        /// </exception>
        public Task CreateAsync(bool recursive, CancellationToken cancellationToken = default) =>
            CreateAsync(recursive, DefaultCreationCollisionOption, cancellationToken);

        /// <summary>
        ///     Creates the element if it does not already exist.
        ///     An exception is thrown if one of the element's parent folders does not exist.
        /// </summary>
        /// <param name="options">
        ///     Defines how to react if another element with a conflicting name already exists in the
        ///     current folder.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="CreateAsync(bool, CreationCollisionOption, CancellationToken)"/> with the
        ///     <c>recursive: false</c> parameter.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        ///     
        ///     -or-
        ///     
        ///     The element already exists and <paramref name="options"/> has the value
        ///     <see cref="CreationCollisionOption.Fail"/>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        /// </exception>
        public Task CreateAsync(CreationCollisionOption options, CancellationToken cancellationToken = default) =>
            CreateAsync(recursive: false, options, cancellationToken);

        /// <summary>
        ///     Creates the element and optionally all of its non-existing parent folders if it does not already exist.
        /// </summary>
        /// <param name="recursive">
        ///     A value indicating whether the element's parent folder should be created recursively
        ///     if they don't already exist.
        ///     
        ///     If <see langword="true"/>, every parent folder which does not exist will be created
        ///     until a parent folder which already exists is reached.
        ///     
        ///     If <see langword="false"/>, this method will throw a <see cref="DirectoryNotFoundException"/>
        ///     if the element's parent folder does not exist.
        /// </param>
        /// <param name="options">
        ///     Defines how to react if another element with a conflicting name already exists in the
        ///     current folder.
        ///     
        ///     These options do not apply to any parents which are created when <paramref name="recursive"/>
        ///     is <see langword="true"/>. They only apply to the creation of this element.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        ///     
        ///     -or-
        ///     
        ///     The element already exists and <paramref name="options"/> has the value
        ///     <see cref="CreationCollisionOption.Fail"/>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist and <paramref name="recursive"/>
        ///     is <see langword="false"/>.
        /// </exception>
        public abstract Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        ///     Deletes the element (and all of its children if it is a <see cref="StorageFolder"/>).
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="DeleteAsync(DeletionOption, CancellationToken)"/> with the
        ///     <see cref="DeletionOption.Fail"/> parameter.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The element is a <see cref="StorageFile"/> and does not exist.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The element is a <see cref="StorageFolder"/> and does not exist.
        /// </exception>
        public Task DeleteAsync(CancellationToken cancellationToken = default) =>
            DeleteAsync(DefaultDeletionOption, cancellationToken);

        /// <summary>
        ///     Deletes the element (and all of its children if it is a <see cref="StorageFolder"/>).
        /// </summary>
        /// <param name="options">
        ///     Defines how to react when the element to be deleted does not exist.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to the element is restricted.
        /// </exception>
        /// <exception cref="IOException">
        ///     An I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     The element is a <see cref="StorageFile"/> which does not exist and <paramref name="options"/>
        ///     has the value <see cref="DeletionOption.Fail"/>.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of the element's parent folders does not exist.
        ///     
        ///     -or-
        ///     
        ///     The element is a <see cref="StorageFolder"/> which does not exist and <paramref name="options"/>
        ///     has the value <see cref="DeletionOption.Fail"/>.
        /// </exception>
        public abstract Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns the element's full path as a string.
        /// </summary>
        public sealed override string ToString() =>
            Path.FullPath.ToString();

    }

}
