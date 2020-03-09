namespace Files
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Files.Resources;

    /// <summary>
    ///     Represents an arbitrary file system and provides methods for interacting with its elements.
    /// </summary>
    public abstract class FileSystem
    {

        /// <summary>
        ///     Gets a <see cref="PathInformation"/> instance which provides information about
        ///     the representation of a path in this file system implementation.
        /// </summary>
        public abstract PathInformation PathInformation { get; }

        /// <summary>
        ///     Returns a <see cref="Path"/> instance from a specified <see cref="string"/>.
        /// </summary>
        /// <param name="path">
        ///     The <see cref="string"/> from which a new <see cref="Path"/> instance should be created.
        /// </param>
        /// <returns>
        ///     A <see cref="Path"/> instance created from the specified <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="path"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The <paramref name="path"/> had an invalid format which is not compatible with this
        ///     file system implementation.
        /// </exception>
        public abstract Path GetPath(string path);

        /// <summary>
        ///     Returns a <see cref="Path"/> instance which represents a specific folder
        ///     identified through the <paramref name="knownFolder"/> parameter.
        /// </summary>
        /// <param name="knownFolder">
        ///     A folder for which a path should be retrieved.
        /// </param>
        /// <returns>
        ///     A path instance which represents a specific folder identified through the
        ///     <paramref name="knownFolder"/> parameter.
        /// </returns>
        /// <remarks>
        ///     If not overridden, this method calls <see cref="TryGetPath(KnownFolder, out Path)"/>
        ///     and throws a <see cref="NotSupportedException"/> if the return value is
        ///     <see langword="false"/>.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        ///     The requested folder is not supported by this file system implementation.
        /// </exception>
        public virtual Path GetPath(KnownFolder knownFolder)
        {
            return TryGetPath(knownFolder, out var result)
                ? result
                : throw new NotSupportedException(ExceptionStrings.FileSystem.KnownFolderNotSupported(knownFolder));
        }

        public virtual bool TryGetPath(string? path, [NotNullWhen(true)] out Path? result)
        {
            // Fast path without a (guaranteed) exception.
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }

            try
            {
                result = GetPath(path!);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to return a <see cref="Path"/> instance which represents a specific folder
        ///     identified through the <paramref name="knownFolder"/> parameter.
        ///     
        ///     In comparison to <see cref="GetPath(KnownFolder)"/> this method doesn't throw an
        ///     exception if the specified <see cref="KnownFolder"/> value is not supported by the
        ///     file system.
        /// </summary>
        /// <param name="knownFolder">
        ///     A known folder for which a <see cref="Path"/> instance should be retrieved.
        /// </param>
        /// <param name="result">
        ///     The result of retrieving the path.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the specified <see cref="KnownFolder"/> value is
        ///     supported by the file system;
        ///     <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetPath(KnownFolder)"/>
        public abstract bool TryGetPath(KnownFolder knownFolder, [NotNullWhen(true)] out Path? result);

        /// <summary>
        ///     Returns a <see cref="File"/> instance which represents the file at the specified
        ///     <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>
        ///     A <see cref="File"/> instance which represents the file at the specified <paramref name="path"/>.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="GetFile(Path)"/> with a
        ///     <see cref="Path"/> instance obtained through the <see cref="GetPath(string)"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="path"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The <paramref name="path"/> had an invalid format which is not compatible with this
        ///     file system implementation.
        /// </exception>
        public virtual File GetFile(string path) =>
            GetFile(GetPath(path));

        /// <summary>
        ///     Returns a <see cref="File"/> instance which represents the file at the specified
        ///     <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>
        ///     A <see cref="File"/> instance which represents the file at the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="path"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The specified <paramref name="path"/> instance is a <see cref="Path"/> implementation
        ///     which is not supported by this file system.
        /// </exception>
        public abstract File GetFile(Path path);

        public virtual bool TryGetFile(string? path, [NotNullWhen(true)] out File? result)
        {
            // Fast path without a (guaranteed) exception.
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFile(path!);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031
            {
                result = null;
                return false;
            }
        }

        public virtual bool TryGetFile(Path? path, [NotNullWhen(true)] out File? result)
        {
            // Fast path without a (guaranteed) exception.
            if (path is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFile(path!);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Returns a <see cref="Folder"/> instance which represents the folder at the specified
        ///     <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of the folder.</param>
        /// <returns>
        ///     A <see cref="Folder"/> instance which represents the folder at the specified <paramref name="path"/>.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="GetFolder(Path)"/> with a
        ///     <see cref="Path"/> instance obtained through the <see cref="GetPath(string)"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="path"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The <paramref name="path"/> had an invalid format which is not compatible with this
        ///     file system implementation.
        /// </exception>
        public virtual Folder GetFolder(string path) =>
            GetFolder(GetPath(path));

        /// <summary>
        ///     Returns a <see cref="Folder"/> instance which represents the folder at the specified
        ///     <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of the folder.</param>
        /// <returns>
        ///     A <see cref="Folder"/> instance which represents the folder at the specified <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="path"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The specified <paramref name="path"/> instance is a <see cref="Path"/> implementation
        ///     which is not supported by this file system.
        /// </exception>
        public abstract Folder GetFolder(Path path);

        /// <summary>
        ///     Attempts to return a <see cref="Folder"/> instance instance which represents a
        ///     specific folder identified through the <paramref name="knownFolder"/> parameter.
        /// </summary>
        /// <param name="knownFolder">
        ///     A known folder for which a <see cref="Folder"/> instance should be retrieved.
        /// </param>
        /// <returns>
        ///     A <see cref="Folder"/> instance identified through the <paramref name="knownFolder"/> parameter.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="GetFolder(Path)"/> with a
        ///     <see cref="Path"/> instance obtained through the <see cref="GetPath(string)"/> method.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        ///     The requested folder is not supported by this file system implementation.
        /// </exception>
        public virtual Folder GetFolder(KnownFolder knownFolder) =>
            GetFolder(GetPath(knownFolder));
        
        public virtual bool TryGetFolder(string? path, [NotNullWhen(true)] out Folder? result)
        {
            // Fast path without a (guaranteed) exception.
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFolder(path!);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031
            {
                result = null;
                return false;
            }
        }

        public virtual bool TryGetFolder(Path? path, [NotNullWhen(true)] out Folder? result)
        {
            // Fast path without a (guaranteed) exception.
            if (path is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFolder(path!);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to return a <see cref="Folder"/> instance instance which represents a
        ///     specific folder identified through the <paramref name="knownFolder"/> parameter.
        /// </summary>
        /// <param name="knownFolder">
        ///     The folder to be returned.
        /// </param>
        /// <param name="result">
        ///     The result of retrieving the folder.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the specified <see cref="KnownFolder"/> value is
        ///     supported by the file system;
        ///     <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(KnownFolder)"/>
        public virtual bool TryGetFolder(KnownFolder knownFolder, [NotNullWhen(true)] out Folder? result)
        {
            if (TryGetPath(knownFolder, out var path))
            {
                result = GetFolder(path);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

    }

}
