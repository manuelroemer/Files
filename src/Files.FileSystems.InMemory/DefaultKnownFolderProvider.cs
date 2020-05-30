namespace Files.FileSystems.InMemory
{
    using System;
    using Files.Shared;

    /// <summary>
    ///     The default implementation of the <see cref="IKnownFolderProvider"/> interface.
    ///     
    ///     This implementation stringifies the <see cref="KnownFolder"/> value and then returns
    ///     a full path based on the string.
    /// </summary>
    public sealed class DefaultKnownFolderProvider : IKnownFolderProvider
    {
        /// <summary>
        ///     Gets a default instance of the <see cref="DefaultKnownFolderProvider"/> class.
        /// </summary>
        public static DefaultKnownFolderProvider Default { get; } = new DefaultKnownFolderProvider();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultKnownFolderProvider"/> class.
        /// </summary>
        public DefaultKnownFolderProvider() { }

        /// <summary>
        ///     Converts the <paramref name="knownFolder"/> to a string and then calls the
        ///     <see cref="InMemoryFileSystem.GetPath(string)"/> method with it.
        ///     Then returns the full path of the result.
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
        /// <exception cref="ArgumentException">
        ///     <paramref name="knownFolder"/> is an invalid <see cref="KnownFolder"/> enumeration value.
        /// </exception>
        public StoragePath GetPath(InMemoryFileSystem fileSystem, KnownFolder knownFolder)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            if (!EnumInfo.IsDefined(knownFolder))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(knownFolder), nameof(knownFolder));
            }

            return fileSystem.GetPath(knownFolder.ToString()).FullPath;
        }
    }
}
