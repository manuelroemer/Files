namespace Files
{

    /// <summary>
    ///     Represents an element which is associated with an arbitrary file system.
    /// </summary>
    public interface IFileSystemElement
    {

        /// <summary>
        ///     Gets the file system with which this element is associated.
        /// </summary>
        FileSystem FileSystem { get; }

    }

}
