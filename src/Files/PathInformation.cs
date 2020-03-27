namespace Files
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     Provides information about certain components of a path in a file system implementation.
    /// </summary>
    public sealed class PathInformation
    {
        /// <summary>
        ///     Gets a list of characters which are not allowed to appear in a path targeting this
        ///     file system implementation.
        /// </summary>
        public IReadOnlyList<char> InvalidPathChars { get; }

        /// <summary>
        ///     Gets a list of characters which are not allowed to appear in the file name part of
        ///     a path targeting this file system implementation.
        /// </summary>
        public IReadOnlyList<char> InvalidFileNameChars { get; }

        /// <summary>
        ///     Gets a character which is used by this file system implementation to separate a path
        ///     into its various segments.
        /// </summary>
        public char DirectorySeparatorChar { get; }

        /// <summary>
        ///     Gets an alternative character which is used by this file system implementation to separate a path
        ///     into its various segments.
        ///     
        ///     Depending on the file system implementation, this property may return the same
        ///     character as <see cref="DirectorySeparatorChar"/>.
        /// </summary>
        public char AltDirectorySeparatorChar { get; }

        /// <summary>
        ///     Gets a distinct list which contains the file system's directory separator chars.
        ///     This list contains the <see cref="DirectorySeparatorChar"/> and <see cref="AltDirectorySeparatorChar"/>
        ///     if the characters are different. Otherwise, this list only contains a single directory
        ///     separator character.
        /// </summary>
        public IReadOnlyList<char> DirectorySeparatorChars { get; }

        /// <summary>
        ///     Gets a character which is used by this file system implementation to separate a
        ///     file name from a file extension.
        /// </summary>
        public char ExtensionSeparatorChar { get; }

        /// <summary>
        ///     Gets a character which is used by this file system implementation to separate
        ///     a volume from the rest of the path.
        /// </summary>
        public char VolumeSeparatorChar { get; }

        /// <summary>
        ///     Gets a string which is used by this file system implementation to refer to the
        ///     current directory in a path.
        ///     In most file system implementations, this is the <c>"."</c> string.
        /// </summary>
        public string CurrentDirectorySegment { get; }

        /// <summary>
        ///     Gets a string which is used by this file system implementation to refer to the
        ///     parent directory in a path.
        ///     In most file system implementations, this is the <c>".."</c> string.
        /// </summary>
        public string ParentDirectorySegment { get; }

        /// <summary>
        ///     Gets the <see cref="StringComparison"/> which is, by default, used by this file system
        ///     implementation to compare paths.
        ///     
        ///     Please be aware of the fact that this property is named "Default" for a reason and
        ///     might, in certain situations, not reflect the real string comparison used in a file
        ///     system.
        ///     See remarks for details.
        /// </summary>
        /// <remarks>
        ///     Depending on the file system implementation, it can very well happen that paths
        ///     are compared with different string comparisons in different locations.
        ///     This could, for example, be the case in a real, physical file system which mounts
        ///     another file system which uses a different string comparison. In such cases, this
        ///     property will lead to invalid comparisons.
        ///     For that reason, information about path equality, even with this property, should
        ///     always be treated with care.
        /// </remarks>
        public StringComparison DefaultStringComparison { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathInformation"/> class.
        /// </summary>
        /// <param name="invalidPathChars">
        ///     A list of characters which are not allowed to appear in a path targeting this
        ///     file system implementation.
        /// </param>
        /// <param name="invalidFileNameChars">
        ///     A list of characters which are not allowed to appear in the file name part of
        ///     a path targeting this file system implementation.
        /// </param>
        /// <param name="directorySeparatorChar">
        ///     A character which is used by this file system implementation to separate a path
        ///     into its various segments.
        /// </param>
        /// <param name="altDirectorySeparatorChar">
        ///     An alternative character which is used by this file system implementation to separate a path
        ///     into its various segments.
        /// </param>
        /// <param name="extensionSeparatorChar">
        ///     A character which is used by this file system implementation to separate a
        ///     file name from a file extension.
        /// </param>
        /// <param name="volumeSeparatorChar">
        ///     A character which is used by this file system implementation to separate
        ///     a volume from the rest of the path.
        /// </param>
        /// <param name="currentDirectorySegment">
        ///     A string which is used by this file system implementation to refer to the
        ///     current directory in a path.
        /// </param>
        /// <param name="parentDirectorySegment">
        ///     A string which is used by this file system implementation to refer to the
        ///     parent directory in a path.
        /// </param>
        /// <param name="defaultStringComparison">
        ///     The <see cref="StringComparison"/> which is, by default, used by this file system
        ///     implementation to compare paths.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="invalidPathChars"/>, <paramref name="invalidFileNameChars"/>,
        ///     <paramref name="parentDirectorySegment"/> or <paramref name="currentDirectorySegment"/>
        ///     is <see langword="null"/>.
        /// </exception>
        public PathInformation(
            IEnumerable<char> invalidPathChars,
            IEnumerable<char> invalidFileNameChars,
            char directorySeparatorChar,
            char altDirectorySeparatorChar,
            char extensionSeparatorChar,
            char volumeSeparatorChar,
            string currentDirectorySegment,
            string parentDirectorySegment,
            StringComparison defaultStringComparison)
        {
            _ = invalidPathChars ?? throw new ArgumentNullException(nameof(invalidPathChars));
            _ = invalidFileNameChars ?? throw new ArgumentNullException(nameof(invalidFileNameChars));
            _ = parentDirectorySegment ?? throw new ArgumentNullException(nameof(parentDirectorySegment));
            _ = currentDirectorySegment ?? throw new ArgumentNullException(nameof(currentDirectorySegment));

            InvalidPathChars = new ReadOnlyCollection<char>(invalidPathChars.ToArray());
            InvalidFileNameChars = new ReadOnlyCollection<char>(invalidFileNameChars.ToArray());
            DirectorySeparatorChar = directorySeparatorChar;
            AltDirectorySeparatorChar = altDirectorySeparatorChar;
            ExtensionSeparatorChar = extensionSeparatorChar;
            VolumeSeparatorChar = volumeSeparatorChar;
            CurrentDirectorySegment = currentDirectorySegment;
            ParentDirectorySegment = parentDirectorySegment;
            DefaultStringComparison = defaultStringComparison;

            DirectorySeparatorChars = new[] { DirectorySeparatorChar, AltDirectorySeparatorChar }
                .Distinct()
                .ToList()
                .AsReadOnly();
        }
    }
}
