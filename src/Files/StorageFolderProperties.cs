namespace Files
{
    using System;

    /// <summary>
    ///     Provides basic properties of a folder in a file system.
    /// </summary>
    public sealed class StorageFolderProperties : StorageElementProperties
    {

        // Keep this class even though there are currently no properties here.
        // This ensures that extension in the future is possible without having users of the library
        // replace 'FileSystemElementProperties' passages in their code.

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageFolderProperties"/> class.
        /// </summary>
        /// <param name="name">The real name of the element.</param>
        /// <param name="nameWithoutExtension">The real name of the element without an extension.</param>
        /// <param name="extension">The real extension of the element.</param>
        /// <param name="createdOn">The point in time when the element has been created.</param>
        /// <param name="modifiedOn">The point in time when the element has been modified the last time.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name"/> or <paramref name="nameWithoutExtension"/> is <see langword="null"/>.
        /// </exception>
        public StorageFolderProperties(
            string name,
            string nameWithoutExtension,
            string? extension,
            DateTimeOffset createdOn,
            DateTimeOffset? modifiedOn
        ) : base(name, nameWithoutExtension, extension, createdOn, modifiedOn) { }

    }

}
