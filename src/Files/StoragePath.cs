﻿namespace Files
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Files.Shared;

#pragma warning disable CA1036
    // Override methods on comparable types, i.e. implement <, >, <=, >= operators due to IComparable.
    // These operators are not implemented because .NET's string class also doesn't implement them.
    // Since a path is, at the end, just a string these operators are also not implemented here.

    /// <summary>
    ///     An immutable representation of a path which points to an element in a file system.
    ///     Instances can be created via the <see cref="Files.FileSystem"/> class.
    /// </summary>
    public abstract class StoragePath :
        IFileSystemElement,
        IEquatable<string?>,
        IEquatable<StoragePath?>,
        IComparable,
        IComparable<string?>,
        IComparable<StoragePath?>
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
        ///     Gets a value indicating whether this path is an absolute or relative path.
        /// </summary>
        public abstract PathKind Kind { get; }
        
        /// <summary>
        ///     Gets a <see cref="StoragePath"/> which represent's this path's root directory.
        ///     If the path doesn't have a root directory (for example if it is a relative path),
        ///     this returns <see langword="null"/>.
        /// </summary>
        public abstract StoragePath? Root { get; }

        /// <summary>
        ///     Gets a <see cref="StoragePath"/> which represent's this path's parent directory.
        ///     If the path doesn't have a parent directory (for example if it represents a root
        ///     directory), this returns <see langword="null"/>.
        ///     
        ///     If the path ends with a single trailing directory separator, this separator is ignored
        ///     and the parent is determined based on the segment before that separator.
        /// </summary>
        public abstract StoragePath? Parent { get; }

        /// <summary>
        ///     Gets a <see cref="StoragePath"/> instance which represents a fully resolved, absolute path
        ///     based on this path.
        /// </summary>
        public abstract StoragePath FullPath { get; }

        /// <summary>
        ///     Gets the full name of the path, i.e. the last segment, including the extension.
        ///     
        ///     If the path ends with a single trailing directory separator, this separator is ignored
        ///     and the name is determined based on the segment before that separator.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Gets the name of the path, i.e. the last segment, without the extension.
        ///     
        ///     If the path ends with a single trailing directory separator, this separator is ignored
        ///     and the name is determined based on the segment before that separator.
        /// </summary>
        public abstract string NameWithoutExtension { get; }

        /// <summary>
        ///     Gets the extension of the path without the file system's extension separator.
        ///     If the path doesn't have an extension, this returns <see langword="null"/>.
        ///     
        ///     If the path ends with a single trailing directory separator, this separator is ignored
        ///     and the extension is determined based on the segment before that separator.
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
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string.
        /// </exception>
        protected StoragePath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            if (path.Length == 0)
            {
                throw new ArgumentException(ExceptionStrings.String.CannotBeEmpty(), nameof(path));
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
            catch
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
        ///     This happens when the path consists of one directory separator character only.
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
            catch
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
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc cref="Combine(string)"/>
        public StoragePath Combine(StoragePath other) =>
            Combine(other?._underlyingString!);

        /// <summary>
        ///     Concatenates the two paths while also ensuring that <i>at least one</i> directory separator
        ///     character is inserted between them.
        ///     
        ///     If <paramref name="other"/> is rooted or starts with a directory separator character,
        ///     this path is discarded and the resulting path will simply be <paramref name="other"/>.
        ///     
        ///     See remarks for details and examples.
        /// </summary>
        /// <param name="other">
        ///     Another path to be concatenated with this path.
        /// </param>
        /// <returns>The resulting concatenated path.</returns>
        /// <remarks>
        ///     This method behaves like .NET's <see cref="System.IO.Path.Combine(string, string)"/>
        ///     method.
        ///     
        ///     In comparison to the alternatives (<see cref="Join(string)"/> and <see cref="Link(string)"/>),
        ///     <see cref="Combine(string)"/> discards this path if <paramref name="other"/> is rooted or
        ///     starts with a directory separator character.
        ///     Out of the three methods, <see cref="Combine(string)"/> is the method that might
        ///     remove the most information from the two specified paths.
        ///     
        ///     The following code demonstrates the behavior of <see cref="Combine(string)"/>:
        ///     
        ///     <code>
        ///     // Note: The code assumes that / is the file system's directory separator.
        ///     
        ///     StoragePath first = fs.GetPath("firstPath");
        ///     first.Combine("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Combine("/secondPath");   // Returns "/secondPath".
        ///     first.Combine("///secondPath"); // Returns "///secondPath".
        ///     
        ///     first = fs.GetPath("firstPath/");
        ///     first.Combine("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Combine("/secondPath");   // Returns "/secondPath".
        ///     first.Combine("///secondPath"); // Returns "///secondPath".
        ///     
        ///     first = fs.GetPath("firstPath///");
        ///     first.Combine("secondPath");    // Returns "firstPath///secondPath".
        ///     first.Combine("/secondPath");   // Returns "/secondPath".
        ///     first.Combine("///secondPath"); // Returns "///secondPath".
        ///     </code>
        /// </remarks>
        /// <seealso cref="Combine(StoragePath)"/>
        /// <seealso cref="Join(string)"/>
        /// <seealso cref="Join(StoragePath)"/>
        /// <seealso cref="Link(string)"/>
        /// <seealso cref="Link(StoragePath)"/>
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
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc cref="Join(string)"/>
        public StoragePath Join(StoragePath other) =>
            Join(other?._underlyingString!);

        /// <summary>
        ///     Concatenates the two paths while also ensuring that <i>at least one</i> directory separator
        ///     character is inserted between them.
        ///     
        ///     All leading/trailing directory separator chars of <paramref name="other"/> and this path
        ///     are preserved. Neither path is discarded.
        ///     
        ///     See remarks for details and examples.
        /// </summary>
        /// <param name="other">
        ///     Another path to be concatenated with this path.
        /// </param>
        /// <returns>The resulting concatenated path.</returns>
        /// <remarks>
        ///     This method behaves like .NET's
        ///     <c>System.IO.Path.Join(ReadOnlySpan&lt;char&gt;, ReadOnlySpan&lt;char&gt;)</c> method.
        ///     
        ///     In comparison to the alternatives (<see cref="Combine(string)"/> and <see cref="Link(string)"/>),
        ///     <see cref="Join(string)"/> preserves any leading/trailing directory separator chars of
        ///     <paramref name="other"/> and this path.
        ///     In comparison to <see cref="Combine(string)"/> specifically, neither path is discarded.
        ///     Out of the three methods, <see cref="Join(string)"/> is the safest one as it does not
        ///     remove any characters from either path.
        ///     
        ///     The following code demonstrates the behavior of <see cref="Join(string)"/>:
        ///     
        ///     <code>
        ///     // Note: The code assumes that / is the file system's directory separator.
        ///     
        ///     StoragePath first = fs.GetPath("firstPath");
        ///     first.Join("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Join("/secondPath");   // Returns "firstPath/secondPath".
        ///     first.Join("///secondPath"); // Returns "firstPath///secondPath".
        ///     
        ///     first = fs.GetPath("firstPath/");
        ///     first.Join("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Join("/secondPath");   // Returns "firstPath//secondPath".
        ///     first.Join("///secondPath"); // Returns "firstPath////secondPath".
        ///     
        ///     first = fs.GetPath("firstPath///");
        ///     first.Join("secondPath");    // Returns "firstPath///secondPath".
        ///     first.Join("/secondPath");   // Returns "firstPath////secondPath".
        ///     first.Join("///secondPath"); // Returns "firstPath//////secondPath".
        ///     </code>
        /// </remarks>
        /// <seealso cref="Combine(string)"/>
        /// <seealso cref="Combine(StoragePath)"/>
        /// <seealso cref="Join(StoragePath)"/>
        /// <seealso cref="Link(string)"/>
        /// <seealso cref="Link(StoragePath)"/>
        public abstract StoragePath Join(string other);

        public bool TryLink(StoragePath? other, [NotNullWhen(true)] out StoragePath? result) =>
            TryLink(other?._underlyingString, out result);

        public virtual bool TryLink(string? other, [NotNullWhen(true)] out StoragePath? result)
        {
            if (other is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = Link(other);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc cref="Link(string)"/>
        public StoragePath Link(StoragePath other) =>
            Link(other?._underlyingString!);

        /// <summary>
        ///     Concatenates the two paths while also ensuring that <i>exactly one</i> directory separator
        ///     character is inserted between them.
        ///     
        ///     Excess leading/trailing directory separators are removed from both <paramref name="other"/>
        ///     and this path in order to end up with exactly one separator between them.
        ///     Neither path is discarded.
        ///     
        ///     See remarks for details and examples.
        /// </summary>
        /// <param name="other">
        ///     Another path to be concatenated with this path.
        /// </param>
        /// <returns>The resulting concatenated path.</returns>
        /// <remarks>
        ///     This method currently doesn't have an equivalent in the .NET Framework.
        ///     In essence, it behaves similarly to
        ///     <c>System.IO.Path.Join(ReadOnlySpan&lt;char&gt;, ReadOnlySpan&lt;char&gt;)</c>,
        ///     but with the difference that excess directory separators are removed between the two
        ///     paths.
        ///     For people familiar with JavaScript, the method behaves similar to NodeJS's
        ///     <c>path.join([...paths])</c> method: https://nodejs.org/api/path.html#path_path_join_paths
        ///     
        ///     In comparison to the alternatives (<see cref="Combine(string)"/> and <see cref="Join(string)"/>),
        ///     <see cref="Link(string)"/> removes any leading/trailing directory separator chars of both
        ///     <paramref name="other"/> and this path before concatenating them. This ensures that
        ///     exactly one directory separator character is present between the two paths.
        ///     In comparison to the alternatives, this method is the safest option when dealing
        ///     with user input, as the result will, most likely, be a valid path without an
        ///     excess number of directory separator characters.
        ///     In comparison to <see cref="Join(string)"/> specifically, results like <c>firstPath//secondPath</c>
        ///     are not possible with this method.
        ///     
        ///     The following code demonstrates the behavior of <see cref="Link(string)"/>:
        ///     
        ///     <code>
        ///     // Note: The code assumes that / is the file system's directory separator.
        ///     
        ///     StoragePath first = fs.GetPath("firstPath");
        ///     first.Join("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Join("/secondPath");   // Returns "firstPath/secondPath".
        ///     first.Join("///secondPath"); // Returns "firstPath/secondPath".
        ///     
        ///     first = fs.GetPath("firstPath/");
        ///     first.Join("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Join("/secondPath");   // Returns "firstPath/secondPath".
        ///     first.Join("///secondPath"); // Returns "firstPath/secondPath".
        ///     
        ///     first = fs.GetPath("firstPath///");
        ///     first.Join("secondPath");    // Returns "firstPath/secondPath".
        ///     first.Join("/secondPath");   // Returns "firstPath/secondPath".
        ///     first.Join("///secondPath"); // Returns "firstPath/secondPath".
        ///     </code>
        /// </remarks>
        /// <seealso cref="Combine(string)"/>
        /// <seealso cref="Combine(StoragePath)"/>
        /// <seealso cref="Join(string)"/>
        /// <seealso cref="Join(StoragePath)"/>
        /// <seealso cref="Link(StoragePath)"/>
        public abstract StoragePath Link(string other);

        /// <summary>
        ///     Compares this path with another path based on the path strings.
        ///     The comparison is done using the <see cref="PathInformation.DefaultStringComparison"/>
        ///     of this path's file system.
        /// </summary>
        /// <param name="path">
        ///     Another <see cref="StoragePath"/> or <see cref="string"/> to be compared with this path.
        ///     
        ///     If this is any other object or <see langword="null"/>, the return value is undefined.
        /// </param>
        /// <returns>
        ///     A negative value if this path precedes the other <paramref name="path"/>.
        ///     Zero if the two paths are considered equal.
        ///     A positive value if this path follows the other <paramref name="path"/>.
        /// </returns>
        int IComparable.CompareTo(object? path) => path switch
        {
            StoragePath storagePath => CompareTo(storagePath),
            string pathStr => CompareTo(pathStr),
            null => CompareTo((string?)null),
            _ => throw new ArgumentException(ExceptionStrings.Comparable.TypeIsNotSupported(path.GetType()), nameof(path)),
        };

        /// <inheritdoc cref="CompareTo(string?)"/>
        public int CompareTo(StoragePath? path) =>
            CompareTo(path, FileSystem.PathInformation.DefaultStringComparison);

        /// <inheritdoc cref="CompareTo(string?, StringComparison)"/>
        public int CompareTo(StoragePath? path, StringComparison stringComparison) =>
            CompareTo(path?._underlyingString, stringComparison);

        /// <summary>
        ///     Compares this path with another path based on the path strings.
        ///     The comparison is done using the <see cref="PathInformation.DefaultStringComparison"/>
        ///     of this path's file system.
        /// </summary>
        /// <param name="path">Another path to be compared with this path.</param>
        /// <returns>
        ///     A negative value if this path precedes the other <paramref name="path"/>.
        ///     Zero if the two paths are considered equal.
        ///     A positive value if this path follows the other <paramref name="path"/>.
        /// </returns>
        public int CompareTo(string? path) =>
            CompareTo(path, FileSystem.PathInformation.DefaultStringComparison);

        /// <summary>
        ///     Compares this path with another path based on the path strings.
        ///     The comparison is done using the specified <paramref name="stringComparison"/> value.
        /// </summary>
        /// <param name="path">Another path to be compared with this path.</param>
        /// <param name="stringComparison">
        ///     The <see cref="StringComparison"/> to be used for comparing the two paths.
        /// </param>
        /// <returns>
        ///     A negative value if this path precedes the other <paramref name="path"/>.
        ///     Zero if the two paths are considered equal.
        ///     A positive value if this path follows the other <paramref name="path"/>.
        /// </returns>
        public int CompareTo(string? path, StringComparison stringComparison) =>
            string.Compare(_underlyingString, path, stringComparison);

        /// <summary>
        ///     Compares this path with another path for string equality.
        ///     The comparison is done using the <see cref="PathInformation.DefaultStringComparison"/>
        ///     of this path's file system.
        /// </summary>
        /// <param name="path">
        ///     Another <see cref="StoragePath"/> or <see cref="string"/> to be compared with this path.
        ///     
        ///     If this is any other object or <see langword="null"/>, this method returns <see langword="false"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the two paths are considered equal;
        ///     <see langword="false"/> if not.
        /// </returns>
        public sealed override bool Equals(object? path) => path switch
        {
            string pathStr => Equals(pathStr),
            StoragePath storagePath => Equals(storagePath),
            _ => false
        };

        /// <inheritdoc cref="Equals(string?, StringComparison)"/>
        public bool Equals(StoragePath? path) =>
            Equals(path?._underlyingString, FileSystem.PathInformation.DefaultStringComparison);

        /// <inheritdoc cref="Equals(string?, StringComparison)"/>
        public bool Equals(StoragePath? path, StringComparison stringComparison) =>
            Equals(path?._underlyingString, stringComparison);

        /// <summary>
        ///     Compares this path with another path for string equality.
        ///     The comparison is done using the <see cref="PathInformation.DefaultStringComparison"/>
        ///     of this path's file system.
        /// </summary>
        /// <param name="path">Another path to be compared with this path.</param>
        /// <returns>
        ///     <see langword="true"/> if the two paths are considered equal;
        ///     <see langword="false"/> if not.
        /// </returns>
        public bool Equals(string? path) =>
            Equals(path, FileSystem.PathInformation.DefaultStringComparison);

        /// <summary>
        ///     Compares this path with another path for string equality.
        ///     The comparison is done using the specified <paramref name="stringComparison"/> value.
        /// </summary>
        /// <param name="path">Another path to be compared with this path.</param>
        /// <param name="stringComparison">
        ///     The <see cref="StringComparison"/> to be used for comparing the two paths.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the two paths are considered equal;
        ///     <see langword="false"/> if not.
        /// </returns>
        public bool Equals(string? path, StringComparison stringComparison) =>
            ToString().Equals(path, stringComparison);

        /// <summary>
        ///     Returns a hash code for the path.
        /// </summary>
        /// <returns>
        ///     A hash code for the path.
        ///     This method returns the hash code of the underlying path string.
        /// </returns>
        [DebuggerStepThrough]
        public sealed override int GetHashCode() =>
            _underlyingString.GetHashCode();

        /// <summary>
        ///     Returns the underlying path string with which this path has been initialized.
        /// </summary>
        /// <returns>
        ///     The underlying path string with which this path has been initialized.
        ///     This value is never <see langword="null"/> or empty.
        /// </returns>
        [DebuggerStepThrough]
        public sealed override string ToString() =>
            _underlyingString;

        public static StoragePath operator /(StoragePath path1, StoragePath path2)
        {
            // The called overload validates for null.
            return path1 / path2?._underlyingString!;
        }

        public static StoragePath operator /(StoragePath path1, string path2)
        {
            _ = path1 ?? throw new ArgumentNullException(nameof(path1));
            _ = path2 ?? throw new ArgumentNullException(nameof(path2));
            return path1.Join(path2);
        }

        public static StoragePath operator +(StoragePath path, string part)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            _ = part ?? throw new ArgumentNullException(nameof(part));
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

        /// <summary>
        ///     Implicitly converts the specified <paramref name="path"/> to its underlying path string.
        /// </summary>
        /// <param name="path">The path to be converted.</param>
        /// <returns>
        ///     The path's underlying path string or <see langword="null"/> if <paramref name="path"/>
        ///     is <see langword="null"/>.
        /// </returns>
        [return: NotNullIfNotNull("path")]
        public static implicit operator string?(StoragePath? path) =>
            path?.ToString();
    }
#pragma warning restore CA1036
}
