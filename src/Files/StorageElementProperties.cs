namespace Files
{
    using System;

    /// <summary>
    ///     Provides basic properties of a file or folder in a file system.
    /// </summary>
    /// <seealso cref="StorageFileProperties"/>
    /// <seealso cref="StorageFolderProperties"/>
    public abstract class StorageElementProperties
    {
        /// <summary>
        ///     Gets the real name of the element.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the real name of the element without an extension.
        /// </summary>
        public string NameWithoutExtension { get; }

        /// <summary>
        ///     Gets the real extension of the element without the file system's extension separator.
        ///     If the element doesn't have an extension, this returns <see langword="null"/>.
        /// </summary>
        public string? Extension { get; }

        /// <summary>
        ///     Gets the point in time when the element has been created.
        /// </summary>
        public DateTimeOffset CreatedOn { get; }

        /// <summary>
        ///     Gets the point in time when the element has been modified the last time.
        ///     This may be <see langword="null"/> if the element has not been modified.
        /// </summary>
        public DateTimeOffset? ModifiedOn { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageElementProperties"/> class.
        /// </summary>
        /// <param name="name">The real name of the element.</param>
        /// <param name="nameWithoutExtension">The real name of the element without an extension.</param>
        /// <param name="extension">The real extension of the element.</param>
        /// <param name="createdOn">The point in time when the element has been created.</param>
        /// <param name="modifiedOn">The point in time when the element has been modified the last time.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> or <paramref name="nameWithoutExtension"/> is <see langword="null"/>.
        /// </exception>
        private protected StorageElementProperties(
            string name,
            string nameWithoutExtension,
            string? extension,
            DateTimeOffset createdOn,
            DateTimeOffset? modifiedOn
        )
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            NameWithoutExtension = nameWithoutExtension ?? throw new ArgumentNullException(nameof(nameWithoutExtension));
            Extension = extension;
            CreatedOn = createdOn;
            ModifiedOn = modifiedOn;
        }
    }
}
