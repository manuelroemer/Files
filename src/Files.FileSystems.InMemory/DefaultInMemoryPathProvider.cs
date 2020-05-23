﻿namespace Files.FileSystems.InMemory
{
    using System;
    using Files;
    using Files.Shared;

    public sealed class DefaultInMemoryPathProvider : IInMemoryPathProvider
    {
        private static readonly PathInformation DefaultPathInformation = new PathInformation(
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

        public PathInformation PathInformation { get; }

        public DefaultInMemoryPathProvider()
            : this(DefaultPathInformation) { }

        public DefaultInMemoryPathProvider(PathInformation pathInformation)
        {
            PathInformation = pathInformation ?? throw new ArgumentNullException(nameof(pathInformation));
        }

        public StoragePath GetPath(InMemoryFileSystem fileSystem, string path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new DefaultInMemoryStoragePath(fileSystem, path);
        }

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
