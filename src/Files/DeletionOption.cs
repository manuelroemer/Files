namespace Files
{

    /// <summary>
    ///     Defines different ways to react when an element which does not exist during a deletion attempt.
    /// </summary>
    public enum DeletionOption
    {

        /// <summary>
        ///     If the element does not exist (for example because it has already been deleted)
        ///     the operation should throw an exception.
        /// </summary>
        Fail,

        /// <summary>
        ///     If the element does not exist (for example because it has already been deleted)
        ///     the operation should not fail, but exit without any errors.
        /// </summary>
        IgnoreMissing,

    }

}
