namespace Files
{
    /// <summary>
    ///     Defines different ways to react to name collisions during the creation
    ///     of a new file or folder.
    /// </summary>
    public enum CreationCollisionOption
    {
        /// <summary>
        ///     The method should throw an exception if a file or folder already exists in the same location.
        /// </summary>
        Fail,

        /// <summary>
        ///     The existing file or folder should be overwritten or replaced.
        ///     
        ///     Specifying this flag only replaces elements of the same type.
        ///     If a folder already exists in the location where a file is supposed to be created
        ///     the creation will fail (and vice versa).
        /// </summary>
        ReplaceExisting,

        /// <summary>
        ///     The existing file or folder should be used instead of creating a new one.
        ///     The operation will ignore the creation attempt and finish without modifying the 
        ///     file system.
        ///     
        ///     Specifying this flag only ignores elements of the same type.
        ///     If a folder already exists in the location where a file is supposed to be created
        ///     the creation will fail (and vice versa).
        /// </summary>
        UseExisting,
    }
}
