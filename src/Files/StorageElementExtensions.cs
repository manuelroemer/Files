namespace Files
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Defines extension methods for the <see cref="StorageElement"/> class.
    /// </summary>
    public static class StorageElementExtensions
    {
        /// <summary>
        ///     Creates The <see cref="StorageElement"/>.
        ///     An existing file or folder will be replaced.
        /// </summary>
        /// <param name="element">The <see cref="StorageElement"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageElement.CreateAsync(bool, CreationCollisionOption, CancellationToken)"/>
        ///     with the <c>recursive: false</c> and <see cref="CreationCollisionOption.ReplaceExisting"/>
        ///     parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to The <see cref="StorageElement"/> is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of The <see cref="StorageElement"/>'s path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The <see cref="StorageElement"/> is a <see cref="StorageFile"/> (or <see cref="StorageFolder"/>) and a
        ///     conflicting folder (or file) exists at its path.
        ///     
        ///     -or-
        ///     
        ///     An undefined I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of The <see cref="StorageElement"/>'s parent folders does not exist.
        /// </exception>
        public static Task CreateOrReplaceExistingAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, CreationCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Creates The <see cref="StorageElement"/>.
        ///     An existing file or folder will be preserved and used instead of creating a new one.
        /// </summary>
        /// <param name="element">The <see cref="StorageElement"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageElement.CreateAsync(bool, CreationCollisionOption, CancellationToken)"/>
        ///     with the <c>recursive: false</c> and <see cref="CreationCollisionOption.UseExisting"/>
        ///     parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to The <see cref="StorageElement"/> is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of The <see cref="StorageElement"/>'s path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The <see cref="StorageElement"/> is a <see cref="StorageFile"/> (or <see cref="StorageFolder"/>) and a
        ///     conflicting folder (or file) exists at its path.
        ///     
        ///     -or-
        ///     
        ///     An undefined I/O error occured while interacting with the file system.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        ///     One of The <see cref="StorageElement"/>'s parent folders does not exist.
        /// </exception>
        public static Task CreateOrUseExistingAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: false, CreationCollisionOption.UseExisting, cancellationToken);
        }

        /// <summary>
        ///     Recursively creates The <see cref="StorageElement"/> and all of its non-existing parent folders.
        ///     An exception is thrown if The <see cref="StorageElement"/> already exists.
        /// </summary>
        /// <param name="element">The <see cref="StorageElement"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageElement.CreateAsync(bool, CreationCollisionOption, CancellationToken)"/>
        ///     with the <c>recursive: true</c> and <see cref="CreationCollisionOption.Fail"/>
        ///     parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to The <see cref="StorageElement"/> is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of The <see cref="StorageElement"/>'s path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The <see cref="StorageElement"/> already exists.
        ///     
        ///     -or-
        ///     
        ///     The <see cref="StorageElement"/> is a <see cref="StorageFile"/> (or <see cref="StorageFolder"/>) and a
        ///     conflicting folder (or file) exists at its path.
        ///     
        ///     -or-
        ///     
        ///     An undefined I/O error occured while interacting with the file system.
        /// </exception>
        public static Task CreateRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.Fail, cancellationToken);
        }

        /// <summary>
        ///     Recursively creates The <see cref="StorageElement"/> and all of its non-existing parent folders.
        ///     An existing file or folder will be replaced.
        /// </summary>
        /// <param name="element">The <see cref="StorageElement"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageElement.CreateAsync(bool, CreationCollisionOption, CancellationToken)"/>
        ///     with the <c>recursive: true</c> and <see cref="CreationCollisionOption.ReplaceExisting"/>
        ///     parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to The <see cref="StorageElement"/> is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of The <see cref="StorageElement"/>'s path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The <see cref="StorageElement"/> is a <see cref="StorageFile"/> (or <see cref="StorageFolder"/>) and a
        ///     conflicting folder (or file) exists at its path.
        ///     
        ///     -or-
        ///     
        ///     An undefined I/O error occured while interacting with the file system.
        /// </exception>
        public static Task CreateOrReplaceExistingRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.ReplaceExisting, cancellationToken);
        }

        /// <summary>
        ///     Recursively creates The <see cref="StorageElement"/> and all of its non-existing parent folders.
        ///     An existing file or folder will be preserved and used instead of creating a new one.
        /// </summary>
        /// <param name="element">The <see cref="StorageElement"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageElement.CreateAsync(bool, CreationCollisionOption, CancellationToken)"/>
        ///     with the <c>recursive: true</c> and <see cref="CreationCollisionOption.UseExisting"/>
        ///     parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to The <see cref="StorageElement"/> is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of The <see cref="StorageElement"/>'s path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The <see cref="StorageElement"/> is a <see cref="StorageFile"/> (or <see cref="StorageFolder"/>) and a
        ///     conflicting folder (or file) exists at its path.
        ///     
        ///     -or-
        ///     
        ///     An undefined I/O error occured while interacting with the file system.
        /// </exception>
        public static Task CreateOrUseExistingRecursivelyAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.CreateAsync(recursive: true, CreationCollisionOption.UseExisting, cancellationToken);
        }

        /// <summary>
        ///     Deletes The <see cref="StorageElement"/> (and all of its children if it is a <see cref="StorageFolder"/>).
        ///     If the <see cref="StorageElement"/> or one of its parent folders does not exist,
        ///     the method exits without errors.
        /// </summary>
        /// <param name="element">The <see cref="StorageElement"/>.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <remarks>
        ///     Calling this method is equivalent to calling
        ///     <see cref="StorageElement.DeleteAsync(DeletionOption, CancellationToken)"/>
        ///     with the <see cref="DeletionOption.IgnoreMissing"/> parameter.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was cancelled via the specified <paramref name="cancellationToken"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Access to The <see cref="StorageElement"/> is restricted.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The length of The <see cref="StorageElement"/>'s path exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="IOException">
        ///     The <see cref="StorageElement"/> is a <see cref="StorageFile"/> (or <see cref="StorageFolder"/>) and a
        ///     conflicting folder (or file) exists at its path.
        ///     
        ///     -or-
        ///     
        ///     An undefined I/O error occured while interacting with the file system.
        /// </exception>
        public static Task DeleteOrIgnoreMissingAsync(
            this StorageElement element,
            CancellationToken cancellationToken = default
        )
        {
            _ = element ?? throw new ArgumentNullException(nameof(element));
            return element.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken);
        }
    }
}
