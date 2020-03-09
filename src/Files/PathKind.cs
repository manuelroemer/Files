namespace Files
{

    /// <summary>
    ///     Defines whether a path is relative or absolute.
    /// </summary>
    public enum PathKind
    {

        // Use 1 and 2 so that this enum is convertible to System.UriKind.
        // While it's probably never going to be used, it at least ensures future compatibility.

        /// <summary>
        ///     The path is an absolute path.
        /// </summary>
        Absolute = 1,

        /// <summary>
        ///     The path is a relative path.
        /// </summary>
        Relative = 2,

    }

}
