namespace Files.Specification.Tests
{

    /// <summary>
    ///     Creates a new path which is built on the given base path.
    /// </summary>
    /// <param name="basePath">
    ///     Another path which should be used as the parent path of the new path which will be returned.
    /// </param>
    public delegate Path PathProvider(Path basePath);

}
