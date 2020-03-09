namespace Files
{

    /// <summary>
    ///     Represents an item which is associated with an arbitrary file system.
    /// </summary>
    public interface IFileSystemItem
    {

        /// <summary>
        ///     Gets the file system with which this item is associated.
        /// </summary>
        FileSystem FileSystem { get; }

    }

}
