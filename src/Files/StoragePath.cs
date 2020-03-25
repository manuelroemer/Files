namespace Files
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Files.Resources;

    /// <summary>
    ///     An immutable representation of a path which points to an element in a file system.
    /// </summary>
    public abstract class StoragePath : IFileSystemElement, IEquatable<string?>, IEquatable<StoragePath?>
    {

        private readonly string _underlyingString;

        /// <inheritdoc/>
        public abstract FileSystem FileSystem { get; }

        /// <summary>
        ///     Gets the number of characters in the underlying string with which this path has
        ///     been initialized.
        /// </summary>
        public int Length => _underlyingString.Length;

        /// <summary>
        ///     Gets a value indicating whether this instance is an absolute or relative path.
        /// </summary>
        public abstract PathKind Kind { get; }
        
        /// <summary>
        ///     Gets a <see cref="StoragePath"/> instance which represent's this path's root directory.
        ///     If the path doesn't have a root directory (for example if it is a relative path),
        ///     this returns <see langword="null"/>.
        /// </summary>
        public abstract StoragePath? Root { get; }

        /// <summary>
        ///     Gets a <see cref="StoragePath"/> instance which represent's this path's parent directory.
        ///     If the path doesn't have a parent directory (for example if it points to a root
        ///     directory), this returns <see langword="null"/>.
        ///     
        ///     Any trailing directory separator characters are ignored when determining the parent.
        /// </summary>
        public abstract StoragePath? Parent { get; }

        /// <summary>
        ///     Gets a <see cref="StoragePath"/> instance which represents a fully resolved, absolute path
        ///     which is derived from this path.
        /// </summary>
        public abstract StoragePath FullPath { get; }

        /// <summary>
        ///     Gets the base name of the path, i.e. its last portion.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets the base name of the path without an extension.
        /// </summary>
        public abstract string NameWithoutExtension { get; }

        /// <summary>
        ///     Gets the extension of the path without the file system's extension separator.
        ///     If the path doesn't have an extension, this returns <see langword="null"/>.
        /// </summary>
        public abstract string? Extension { get; }

        /// <summary>
        ///     Gets a value indicating whether the path ends in a directory separator character.
        /// </summary>
        public abstract bool EndsInDirectorySeparator { get; }

        /// <summary>
        ///     Initializes a new <see cref="StoragePath"/> instance from the specified <paramref name="path"/> string.
        /// </summary>
        /// <param name="path">
        ///     The string on which this <see cref="StoragePath"/> instance is based.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     * <paramref name="path"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string.
        /// </exception>
        protected StoragePath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            if (path.Length == 0)
            {
                throw new ArgumentException(ExceptionStrings.Path.NotEmpty(), nameof(path));
            }
            
            _underlyingString = path;
        }

        public virtual bool TryTrimEndingDirectorySeparator([NotNullWhen(true)] out StoragePath? result)
        {
            if (!EndsInDirectorySeparator)
            {
                result = this;
                return true;
            }

            try
            {
                result = TrimEndingDirectorySeparator();
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
        ///     Trims one trailing directory separator character from this path and returns the
        ///     resulting path.
        /// </summary>
        /// <returns>
        ///     A new <see cref="StoragePath"/> instance where one trailing directory separator has been
        ///     trimmed.
        ///     If this path doesn't have a trailing directory separator character, the same path
        ///     instance is returned.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Trimming the trailing directory separator character is not possible.
        ///     This can happen when the path consists of one directory separator character only.
        /// </exception>
        public abstract StoragePath TrimEndingDirectorySeparator();

        public virtual bool TryAppend(string? part, [NotNullWhen(true)] out StoragePath? result)
        {
            if (part is null)
            {
                result = null;
                return false;
            }
            
            try
            {
                result = Append(part);
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
        ///     Appends the specified <paramref name="part"/> string to the end of the path.
        /// </summary>
        /// <param name="part">
        ///     The part to be appended to the path.
        /// </param>
        /// <returns>
        ///     A new <see cref="StoragePath"/> instance which represents the path after appending the
        ///     specified <paramref name="part"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="part"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Appending <paramref name="part"/> would result in an invalid path.
        /// </exception>
        public abstract StoragePath Append(string part);

        public bool TryCombine(StoragePath? other, [NotNullWhen(true)] out StoragePath? result) =>
            TryCombine(other?._underlyingString, out result);
        
        public virtual bool TryCombine(string? other, [NotNullWhen(true)] out StoragePath? result)
        {
            if (other is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = Combine(other);
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

        public StoragePath Combine(StoragePath other) =>
            Combine(other?._underlyingString!);

        public abstract StoragePath Combine(string other);

        public bool TryJoin(StoragePath? other, [NotNullWhen(true)] out StoragePath? result) =>
            TryJoin(other?._underlyingString, out result);

        public virtual bool TryJoin(string? other, [NotNullWhen(true)] out StoragePath? result)
        {
            if (other is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = Join(other);
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

        public StoragePath Join(StoragePath other) =>
            Join(other?._underlyingString!);

        public abstract StoragePath Join(string other);

        public sealed override bool Equals(object? obj) => obj switch
        {
            string path => Equals(path),
            StoragePath path => Equals(path),
            _ => false
        };

        public bool Equals(StoragePath? path) =>
            Equals(path?._underlyingString, FileSystem.PathInformation.DefaultStringComparison);

        public bool Equals(StoragePath? path, StringComparison stringComparison) =>
            Equals(path?._underlyingString, stringComparison);

        public bool Equals(string? path) =>
            Equals(path, FileSystem.PathInformation.DefaultStringComparison);

        public bool Equals(string? path, StringComparison stringComparison) =>
            ToString().Equals(path, stringComparison);

        /// <summary>
        ///     Returns a hash code for the path.
        /// </summary>
        /// <returns>
        ///     A hash code for the path.
        ///     This method returns the hash code of the underlying path string.
        /// </returns>
        public sealed override int GetHashCode() =>
            _underlyingString.GetHashCode();

        /// <summary>
        ///     Returns the string representation with which this path has been initialized.
        /// </summary>
        /// <returns>
        ///     The string representation with which this path has been initialized.
        ///     This value is never <see langword="null"/> or empty.
        /// </returns>
        [DebuggerStepThrough]
        public sealed override string ToString() =>
            _underlyingString;

        public static StoragePath operator /(StoragePath path1, StoragePath path2)
        {
            return path1 / path2?._underlyingString!;
        }

        public static StoragePath operator /(StoragePath path1, string path2)
        {
            _ = path1 ?? throw new ArgumentNullException(nameof(path1));
            // JoinWith should validate path2.
            return path1.Join(path2);
        }

        public static StoragePath operator +(StoragePath path, string part)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            // Append should validate part.
            return path.Append(part);
        }

        public static bool operator ==(StoragePath? path1, StoragePath? path2) =>
            path1?.Equals(path2) ?? path2?.Equals(path1) ?? true;

        public static bool operator !=(StoragePath? path1, StoragePath? path2) =>
            !(path1 == path2);

        public static bool operator ==(StoragePath? path1, string? path2) =>
            path1?.Equals(path2) ?? path2 is null;

        public static bool operator !=(StoragePath? path1, string? path2) =>
            !(path1 == path2);

        public static bool operator ==(string? path1, StoragePath? path2) =>
            path2?.Equals(path1) ?? path1 is null;

        public static bool operator !=(string? path1, StoragePath? path2) =>
            !(path1 == path2);

        [return: NotNullIfNotNull("path")]
        public static implicit operator string?(StoragePath? path) =>
            path?.ToString();

    }

}
