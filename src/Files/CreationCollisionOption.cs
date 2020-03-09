namespace Files
{

    /// <summary>
    ///     Defines different ways to react to file or folder name collisions during the creation
    ///     of a new element.
    /// </summary>
    public enum CreationCollisionOption
    {
        
        /// <summary>
        ///     The method should throw an exception if a file or folder already exists at the same location.
        /// </summary>
        Fail = 0,

        /// <summary>
        ///     The existing file or folder should be overwritten or replaced.
        /// </summary>
        ReplaceExisting = 1,

        /// <summary>
        ///     The existing file or folder should be used instead of creating a new one.
        ///     The operation will ignore the creation attempt and finish without modifying the 
        ///     file system.
        /// </summary>
        Ignore = 2,

    }

}
