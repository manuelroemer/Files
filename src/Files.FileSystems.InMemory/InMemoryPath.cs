namespace Files.FileSystems.InMemory
{
    using System;
    using Files;

    public abstract class InMemoryPath : Path
    {

        public sealed override FileSystem FileSystem { get; }

        /// <inheritdoc/>
        protected InMemoryPath(string path, InMemoryFileSystem fileSystem)
            : base(path)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        ///     Normalizes the path.
        /// </summary>
        /// <returns>
        ///     A new <see cref="InMemoryPath"/> instance which points to the same location as this
        ///     path, but is normalized.
        /// </returns>
        public abstract InMemoryPath Normalize();

        /// <summary>
        ///     Returns a value indicating whether the given path points to the same location in a
        ///     file system as this path, i.e. if the two paths reference the same element in a
        ///     file system tree.
        /// </summary>
        /// <param name="other">Another path.</param>
        /// <returns>
        ///     <see langword="true"/> if the two paths point to the same location in a file system;
        ///     <see langword="false"/> if not, i.e. if the two paths reference two different, unique elements.
        /// </returns>
        public abstract bool LocationEquals(Path other);

        public abstract bool IsParentPathOf(Path other);

    }

}
