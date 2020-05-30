namespace Files.FileSystems.InMemory
{
    using System;

    /// <summary>
    ///     Represents a member which is able to create <see cref="StoragePath"/> instances
    ///     of a single type on behalf of the <see cref="InMemoryFileSystem"/>.
    ///     
    ///     Implementing this interface allows you to provide the <see cref="StoragePath"/>
    ///     instances which are used by an <see cref="InMemoryFileSystem"/>, thereby enabling you
    ///     to provide customized path behavior.
    /// </summary>
    public interface IInMemoryStoragePathProvider
    {
        /// <summary>
        ///     Gets a <see cref="Files.PathInformation"/> instance which provides information about
        ///     special path characteristics of the <see cref="StoragePath"/> instances created by
        ///     this provider.
        ///     
        ///     This property is returned by the <see cref="InMemoryFileSystem"/>'s
        ///     <see cref="FileSystem.PathInformation"/> property.
        /// </summary>
        PathInformation PathInformation { get; }

        /// <summary>
        ///     Creates and returns a <see cref="StoragePath"/> instance for the specified <paramref name="fileSystem"/>
        ///     from a string.
        ///     
        ///     This method is called by the <see cref="InMemoryFileSystem.GetPath(string)"/> method.
        /// </summary>
        /// <param name="fileSystem">
        ///     An <see cref="InMemoryFileSystem"/> instance for which the <see cref="KnownFolder"/>
        ///     should be located.
        /// </param>
        /// <param name="path">
        ///     The string from which a new <see cref="StoragePath"/> instance should be created.
        /// </param>
        /// <returns>
        ///     A new <see cref="StoragePath"/> instance created from the specified  <paramref name="path"/> string.
        /// </returns>
        /// <remarks>
        ///     When implementing this method, make sure that you don't call the
        ///     <see cref="InMemoryFileSystem.GetPath(string)"/> or <see cref="InMemoryFileSystem.GetPath(KnownFolder)"/>
        ///     method as doing so will usually result in a <see cref="StackOverflowException"/>.
        ///     
        ///     You must also ensure that you always use the specified <paramref name="fileSystem"/>
        ///     to create the <see cref="StoragePath"/>, because each <see cref="InMemoryFileSystem"/>
        ///     is only compatible with those paths that have been created by itself.
        ///     
        ///     If you cannot create a <see cref="StoragePath"/> from the specified <paramref name="path"/>, you
        ///     can throw one of the listed exceptions.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="fileSystem"/> or <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string or has an otherwise invalid format.
        /// </exception>
        StoragePath GetPath(InMemoryFileSystem fileSystem, string path);
    }
}
