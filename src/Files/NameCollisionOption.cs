namespace Files
{
    /// <summary>
    ///     Defines different ways to react to name collisions during file and folder operations.
    /// </summary>
    public enum NameCollisionOption
    {
        /// <summary>
        ///     The method should throw an exception if a file or folder already exists at the destination.
        /// </summary>
        Fail = CreationCollisionOption.Fail,

        /// <summary>
        ///     Any existing file or folder at the destination should be replaced.
        ///     
        ///     Specifying this flag only replaces elements of the same type.
        ///     If a folder already exists in the location where a file is supposed to be copied or moved,
        ///     the operation will fail (and vice versa).
        /// </summary>
        ReplaceExisting = CreationCollisionOption.ReplaceExisting,
    }
}
