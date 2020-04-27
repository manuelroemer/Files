namespace Files
{
    /// <summary>
    ///     Represents an element which is associated with an arbitrary file system.
    ///     See remarks for details.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IFileSystemElement"/> interface is implemented by every member
    ///     which can be created by the <see cref="Files.FileSystem"/> class.
    ///     The interface gives you access to the file system with which the implementing element
    ///     is associated. This enables you to easily create other elements of the same file system
    ///     implementation without direct access to the <see cref="Files.FileSystem"/> instance:
    ///     
    ///     <code>
    ///     StorageFile GetFileFromPath(StoragePath path)
    ///     {
    ///         FileSystem fs = path.FileSystem;
    ///         return fs.GetFile(path);
    ///     }
    ///     </code>
    /// </remarks>
    public interface IFileSystemElement
    {
        /// <summary>
        ///     Gets the file system with which this element is associated.
        /// </summary>
        FileSystem FileSystem { get; }
    }
}
