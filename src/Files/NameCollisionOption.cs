namespace Files
{

    /// <summary>
    ///     Defines different ways to react to file or folder name collisions.
    /// </summary>
    public enum NameCollisionOption
    {

        /// <summary>
        ///     The method should throw an exception if a file or folder already exists at the same 
        ///     location.
        /// </summary>
        Fail = CreationCollisionOption.Fail,

        /// <summary>
        ///     The existing file or folder should be overwritten or replaced.
        ///     
        ///     Specifying this flag only replaces elements of the same type.
        ///     If a folder already exists in the location where a file is supposed to be created,
        ///     the creation will fail (and vice versa).
        /// </summary>
        ReplaceExisting = CreationCollisionOption.ReplaceExisting,

    }

}
