namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using Files;

    /// <summary>
    ///     The default <see cref="StoragePath"/> equality comparer implementation.
    ///     This comparer allows two <see cref="StoragePath"/> instances to locate the same element within
    ///     a file system if their fully resolved counterparts without any trailing directory separators
    ///     are equal with respect to the file system's default string comparison.
    ///     See remarks for details.
    /// </summary>
    /// <remarks>
    ///     This comparer uses the following steps to compare paths:
    ///     
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 Fully resolve the path via the <see cref="StoragePath.FullPath"/> property.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Trim all trailing directory separator characters from the full path (if possible).
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Compare the result with the second path (which has also been modified via
    ///                 these steps) via the <see cref="StoragePath.Equals(StoragePath?)"/> method.
    ///                 If the paths are equal, they locate the same element.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     
    ///     You can inherit from this class and override the <see cref="EqualsCore(StoragePath, StoragePath)"/>
    ///     and/or <see cref="GetHashCodeCore(StoragePath)"/> methods to tighten the default
    ///     equality comparison of this class.
    ///     By inheriting from this class, you gain the benefit of not having to reimplement
    ///     the conversion of a normal <see cref="StoragePath"/> to a fully resolved <see cref="StoragePath"/>
    ///     without trailing directory separator characters.
    /// </remarks>
    public class DefaultStoragePathEqualityComparer : EqualityComparer<StoragePath?>
    {
        /// <summary>
        ///     Gets a default instance of the <see cref="DefaultStoragePathEqualityComparer"/> class.
        /// </summary>
        public static new DefaultStoragePathEqualityComparer Default { get; } = new DefaultStoragePathEqualityComparer();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultStoragePathEqualityComparer"/> class.
        /// </summary>
        public DefaultStoragePathEqualityComparer() { }

        /// <summary>
        ///     Returns a value indicating whether the two specified paths are equal in the sense
        ///     that they locate the same element within a file system.
        /// </summary>
        /// <param name="path1">The first path.</param>
        /// <param name="path2">The second path.</param>
        /// <returns>
        ///     <see langword="true"/> if the two paths locate the same element in a file system;
        ///     <see langword="false"/> if not.
        /// </returns>
        public sealed override bool Equals(StoragePath? path1, StoragePath? path2)
        {
            if (path1 is null && path2 is null)
            {
                return true;
            }

            if (path1 is not null && path2 is not null)
            {
                return EqualsCore(GetFinalPath(path1),  GetFinalPath(path2));
            }

            return false;
        }

        /// <summary>
        ///     If not overridden, determines whether the two specified paths are equal based on the
        ///     <see cref="StoragePath.Equals(StoragePath?)"/> method.
        /// </summary>
        /// <param name="fullPath1">
        ///     A fully resolved <see cref="StoragePath"/> without trailing directory separators which
        ///     should be compared with <paramref name="fullPath2"/>.
        /// </param>
        /// <param name="fullPath2">
        ///     A fully resolved <see cref="StoragePath"/> without trailing directory separators which
        ///     should be compared with <paramref name="fullPath1"/>.
        /// </param>
        /// <returns>
        ///     The result of calling <see cref="StoragePath.Equals(StoragePath?)"/> on the two
        ///     specified paths.
        /// </returns>
        protected virtual bool EqualsCore(StoragePath fullPath1, StoragePath fullPath2)
        {
            _ = fullPath1 ?? throw new ArgumentNullException(nameof(fullPath1));
            _ = fullPath2 ?? throw new ArgumentNullException(nameof(fullPath2));
            return fullPath1.Equals(fullPath2);
        }
        
        /// <summary>
        ///     Returns a hash code for the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path for which a hash code should be returned.</param>
        /// <returns>
        ///     <c>0</c> if <paramref name="path"/> is <see langword="null"/>;
        ///     the result of <see cref="GetHashCodeCore(StoragePath)"/> otherwise.
        /// </returns>
        public sealed override int GetHashCode(StoragePath? path)
        {
            return path is null ? 0 : GetHashCodeCore(GetFinalPath(path));
        }

        /// <summary>
        ///     If not overridden, returns the hash code of the specified <paramref name="normalizedFullPath"/>.
        /// </summary>
        /// <param name="normalizedFullPath">
        ///     A fully resolved <see cref="StoragePath"/> without trailing directory separators for
        ///     which a hash code should be returned.
        /// </param>
        /// <returns>
        ///     The hash code of the specified <paramref name="normalizedFullPath"/>.
        /// </returns>
        protected virtual int GetHashCodeCore(StoragePath normalizedFullPath)
        {
            _ = normalizedFullPath ?? throw new ArgumentNullException(nameof(normalizedFullPath));
            return normalizedFullPath.GetHashCode();
        }

        private static StoragePath GetFinalPath(StoragePath path)
        {
            // Since we don't know the specifics of the underlying path, we've got to settle
            // for the next best comparison possible which is a string-based comparison.
            // Here, always utilize the FullPath and trim unnecessary trailing separators.
            var result = path.FullPath;
            while (result.EndsWithDirectorySeparator && result.TryTrimEndingDirectorySeparator(out var trimmedResult))
            {
                result = trimmedResult;
            }
            return result;
        }
    }
}
