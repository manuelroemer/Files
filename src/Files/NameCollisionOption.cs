namespace Files
{

    /// <summary>
    ///     Defines different ways to react to file or folder name collisions.
    /// </summary>
    public enum NameCollisionOption
    {
        // The numbers here are important, because it allows casting NameCollisionOption to
        // CreationCollisionOption. Always keep them in sync!

        /// <summary>
        ///     The method should throw an exception if a file or folder already exists at the same 
        ///     location.
        /// </summary>
        Fail = 0,

        /// <summary>
        ///     The existing file or folder should be overwritten or replaced.
        /// </summary>
        ReplaceExisting = 1,

    }

}
