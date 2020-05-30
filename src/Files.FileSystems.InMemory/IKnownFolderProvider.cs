namespace Files.FileSystems.InMemory
{
    using System;

    /// <summary>
    ///     Represents a member which is able to return a <see cref="StoragePath"/> that locates
    ///     a specific <see cref="KnownFolder"/> value.
    ///     
    ///     Implementing this interface allows you to provide the <see cref="KnownFolder"/> to
    ///     <see cref="StoragePath"/> mapping which is used by an <see cref="InMemoryFileSystem"/>.
    /// </summary>
    public interface IKnownFolderProvider
    {
        /// <summary>
        ///     Returns a <see cref="StoragePath"/> which locates the specified <paramref name="knownFolder"/>
        ///     within the provided <paramref name="fileSystem"/>.
        ///     
        ///     This method is called by the <see cref="InMemoryFileSystem.GetPath(KnownFolder)"/> method.
        /// </summary>
        /// <param name="fileSystem">
        ///     An <see cref="InMemoryFileSystem"/> instance for which the <see cref="KnownFolder"/>
        ///     should be located.
        /// </param>
        /// <param name="knownFolder">
        ///     The <see cref="KnownFolder"/> value to be located for the specified <paramref name="fileSystem"/>.
        /// </param>
        /// <returns>
        ///     A <see cref="StoragePath"/> which locates the specified <paramref name="knownFolder"/>
        ///     within the provided <paramref name="fileSystem"/>.
        /// </returns>
        /// <remarks>
        ///     When implementing this method, make sure that you don't call the
        ///     <see cref="InMemoryFileSystem.GetPath(KnownFolder)"/> method as doing so will lead
        ///     to a <see cref="StackOverflowException"/>.
        ///     You can, however, call the <see cref="InMemoryFileSystem.GetPath(string)"/> method.
        ///     
        ///     You must also ensure that you always use the specified <paramref name="fileSystem"/>
        ///     to create the <see cref="StoragePath"/>, because each <see cref="InMemoryFileSystem"/>
        ///     is only compatible with those paths that have been created by itself.
        ///     
        ///     If you cannot provide a value for the <paramref name="knownFolder"/> parameter, you
        ///     can throw one of the listed exceptions.
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     <paramref name="knownFolder"/> is an invalid <see cref="KnownFolder"/> enumeration value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     The requested folder is not supported by this file system implementation.
        /// </exception>
        StoragePath GetPath(InMemoryFileSystem fileSystem, KnownFolder knownFolder);
    }
}
