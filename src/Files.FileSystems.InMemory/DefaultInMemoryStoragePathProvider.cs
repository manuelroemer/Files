namespace Files.FileSystems.InMemory
{
    using System;
    using Files;

    /// <summary>
    ///     The default implementation of the <see cref="IInMemoryStoragePathProvider"/> interface.
    ///     
    ///     This implementation creates <see cref="StoragePath"/> instances which behave similarly
    ///     to Unix paths.
    ///     While the general path behavior (e.g. the full path resolution) is fixed, other
    ///     characteristics (e.g. the directory separator character(s)) can be modified via
    ///     a custom <see cref="Files.PathInformation"/> value.
    ///     
    ///     See remarks for additional details about the behavior of the <see cref="StoragePath"/>
    ///     instances created by this provider.
    /// </summary>
    /// <remarks>
    ///     This provider creates <see cref="StoragePath"/> instances which behave similarly to
    ///     Unix paths. The following lists shows several example paths which might be created
    ///     by this provider:
    ///     
    ///     <list type="bullet">
    ///         <item><description>
    ///             <c>/</c>: A root path.
    ///         </description></item>
    ///         <item><description>
    ///             <c>/foo</c>: An absolute path.
    ///         </description></item>
    ///         <item><description>
    ///             <c>foo</c>: A relative path.
    ///         </description></item>
    ///         <item><description>
    ///             <c>/foo/../bar</c>: A path with relative segments.
    ///         </description></item>
    ///         <item><description>
    ///             <c>/foo\bar/baz</c>: A path with multiple directory separators.
    ///         </description></item>
    ///     </list>
    ///     
    ///     The <see cref="StoragePath"/> implementation is also implemented based on top of a
    ///     <see cref="Files.PathInformation"/> value. This means that you can easily adapt most
    ///     parts of the path interpretation, for example which characters are used as directory
    ///     separators.
    ///     
    ///     Apart from these interchangeable values, the <see cref="StoragePath"/> follows the
    ///     traditional path structures. Each path is split into a set of segments, might have
    ///     a name and/or extension, can be fully resolved, etc.
    ///     
    ///     Nontheless, the implementation is slimmed down in comparison to "real" file system paths.
    ///     For example, it does not support special formats like UNC paths or similar. Despite that,
    ///     the entire <see cref="StoragePath"/> contract is fulfilled. Given that your application
    ///     or tests are correctly implemented on top of the abstractions established by the Files
    ///     library, you will be able to switch between a real <see cref="StoragePath"/> implementation
    ///     and this one without any problems.
    /// </remarks>
    public sealed class DefaultInMemoryStoragePathProvider : IInMemoryStoragePathProvider
    {
        private static readonly PathInformation OrdinalPathInformation = new PathInformation(
            invalidPathChars: Array.Empty<char>(),
            invalidFileNameChars: Array.Empty<char>(),
            directorySeparatorChar: '/',
            altDirectorySeparatorChar: '\\',
            extensionSeparatorChar: '.',
            volumeSeparatorChar: '/',
            currentDirectorySegment: ".",
            parentDirectorySegment: "..",
            defaultStringComparison: StringComparison.Ordinal
        );

        private static readonly PathInformation OrdinalIgnoreCasePathInformation = new PathInformation(
            invalidPathChars: Array.Empty<char>(),
            invalidFileNameChars: Array.Empty<char>(),
            directorySeparatorChar: '/',
            altDirectorySeparatorChar: '\\',
            extensionSeparatorChar: '.',
            volumeSeparatorChar: '/',
            currentDirectorySegment: ".",
            parentDirectorySegment: "..",
            defaultStringComparison: StringComparison.OrdinalIgnoreCase
        );

        /// <summary>
        ///     Gets a default <see cref="DefaultInMemoryStoragePathProvider"/> instance which
        ///     uses the case-sensitive <see cref="StringComparison.Ordinal"/> value to compare paths.
        ///     In most cases, this results in a case-sensitive <see cref="InMemoryFileSystem"/> instance.
        /// </summary>
        public static DefaultInMemoryStoragePathProvider DefaultOrdinal { get; } = 
            new DefaultInMemoryStoragePathProvider(OrdinalPathInformation);

        /// <summary>
        ///     Gets a default <see cref="DefaultInMemoryStoragePathProvider"/> instance which
        ///     uses the case-insensitive <see cref="StringComparison.OrdinalIgnoreCase"/> value to compare paths.
        ///     In most cases, this results in a case-insensitive <see cref="InMemoryFileSystem"/> instance.
        /// </summary>
        public static DefaultInMemoryStoragePathProvider DefaultOrdinalIgnoreCase { get; } =
            new DefaultInMemoryStoragePathProvider(OrdinalIgnoreCasePathInformation);

        /// <inheritdoc/>
        public PathInformation PathInformation { get; }

        /// <summary>
        ///     Initializes a new <see cref="DefaultInMemoryStoragePathProvider"/> instance which
        ///     uses the case-sensitive <see cref="StringComparison.Ordinal"/> value to compare paths.
        ///     In most cases, this results in a case-sensitive <see cref="InMemoryFileSystem"/> instance.
        ///     
        ///     Using this parameterless constructor results in an instance equivalent to <see cref="DefaultOrdinal"/>.
        /// </summary>
        public DefaultInMemoryStoragePathProvider()
            : this(OrdinalPathInformation) { }

        /// <summary>
        ///     Initializes a new <see cref="DefaultInMemoryStoragePathProvider"/> instance with
        ///     a custom <see cref="Files.PathInformation"/> value which is supposed to be used
        ///     by the <see cref="StoragePath"/> instances which will be created by this provider.
        /// </summary>
        /// <param name="pathInformation">
        ///     A custom <see cref="Files.PathInformation"/> value which is supposed to be used
        ///     by the <see cref="StoragePath"/> instances which will be created by this provider.
        ///     
        ///     Via this value, you can define the exact characters which are used in the
        ///     <see cref="StoragePath"/> algorithms. For example, you can change the default
        ///     directory separator characters or the invalid path characters.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="pathInformation"/> is <see langword="null"/>.
        /// </exception>
        public DefaultInMemoryStoragePathProvider(PathInformation pathInformation)
        {
            PathInformation = pathInformation ?? throw new ArgumentNullException(nameof(pathInformation));
        }

        /// <inheritdoc/>
        public StoragePath GetPath(InMemoryFileSystem fileSystem, string path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new DefaultInMemoryStoragePath(fileSystem, path);
        }
    }
}
