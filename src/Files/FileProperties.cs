namespace Files
{
    using System;

    /// <summary>
    ///     Provides basic properties of a file in a file system.
    /// </summary>
    public sealed class FileProperties : FileSystemElementProperties
    {

        /// <summary>
        ///     Gets the file's size in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileProperties"/> class.
        /// </summary>
        /// <param name="name">The real name of the element.</param>
        /// <param name="nameWithoutExtension">The real name of the element without an extension.</param>
        /// <param name="extension">The real extension of the element.</param>
        /// <param name="createdOn">The point in time when the element has been created.</param>
        /// <param name="modifiedOn">The point in time when the element has been modified the last time.</param>
        /// <param name="size">The file's size in bytes.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> or <paramref name="nameWithoutExtension"/> is <see langword="null"/>.
        /// </exception>
        public FileProperties(
            string name,
            string nameWithoutExtension,
            string? extension,
            DateTimeOffset createdOn,
            DateTimeOffset? modifiedOn,
            ulong size
        ) : base(name, nameWithoutExtension, extension, createdOn, modifiedOn)
        {
            Size = size;
        }

    }

}
