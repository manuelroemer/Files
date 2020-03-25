namespace Files.FileSystems.Physical
{
    using System;
    using System.IO;
    using Files;
    using Files.FileSystems.Physical.Resources;
    using Files.FileSystems.Physical.Utilities;

    internal sealed class PhysicalStoragePath : StoragePath
    {

        private readonly Lazy<StoragePath?> _rootLazy;
        private readonly Lazy<StoragePath?> _parentLazy;
        private readonly Lazy<StoragePath> _fullPathLazy;
        private readonly Lazy<StoragePath> _pathWithoutEndingDirectorySeparatorLazy;

        public override FileSystem FileSystem { get; }

        public override PathKind Kind { get; }

        public override StoragePath? Root => _rootLazy.Value;

        public override StoragePath? Parent => _parentLazy.Value;

        public override StoragePath FullPath => _fullPathLazy.Value;

        public override string Name { get; }

        public override string NameWithoutExtension { get; }

        public override string? Extension { get; }

        public override bool EndsInDirectorySeparator { get; }

        internal PhysicalStoragePath(PhysicalFileSystem fileSystem, string path)
            : base(path)
        {
            var fullPath = GetFullPathOrThrow(path);
            var rootPath = Path.GetPathRoot(ToString());
            var pathWithoutTrailingSeparator = Path.TrimEndingDirectorySeparator(path);
            var directoryPath = Path.GetDirectoryName(pathWithoutTrailingSeparator);
            var name = Path.GetFileName(pathWithoutTrailingSeparator);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(pathWithoutTrailingSeparator);
            var extension = PathHelper.GetExtensionWithoutTrailingExtensionSeparator(pathWithoutTrailingSeparator);
            var isPathFullyQualified = Path.IsPathFullyQualified(path);
            var endsInDirectorySeparator = Path.EndsInDirectorySeparator(path);

            FileSystem = fileSystem;
            Kind = isPathFullyQualified ? PathKind.Absolute : PathKind.Relative;
            Name = name;
            NameWithoutExtension = nameWithoutExtension;
            Extension = string.IsNullOrEmpty(extension) ? null : extension;
            EndsInDirectorySeparator = endsInDirectorySeparator;

            _rootLazy = new Lazy<StoragePath?>(
                () => string.IsNullOrEmpty(rootPath) ? null : fileSystem.GetPath(rootPath)
            );

            _parentLazy = new Lazy<StoragePath?>(
                () => string.IsNullOrEmpty(directoryPath) ? null : fileSystem.GetPath(directoryPath)
            );

            _fullPathLazy = new Lazy<StoragePath>(
                () => fileSystem.GetPath(fullPath)
            );

            _pathWithoutEndingDirectorySeparatorLazy = new Lazy<StoragePath>(
                () =>
                {
                    if (!EndsInDirectorySeparator)
                    {
                        return this;
                    }

                    var trimmedPath = Path.TrimEndingDirectorySeparator(ToString());

                    try
                    {
                        return FileSystem.GetPath(trimmedPath);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new InvalidOperationException(
                            ExceptionStrings.Path.TrimmingResultsInInvalidPath(),
                            ex
                        );
                    }
                }
            );
        }

        /// <summary>
        ///     Converts the specified path to a full path and throws an appropriate exception
        ///     if that is not possible due to an invalid path format.
        ///     This is both used for path string validation and full path retrieval.
        /// </summary>
        private static string GetFullPathOrThrow(string path)
        {
            // Path rules are incredibly complex depending on the current OS. It's best to not try
            // and emulate these rules here, but to actually use the OS/FS APIs directly.
            // One way to do this is by calling GetFullPath() and checking whether that throws.
            // If not, the path is good.
            // It certainly looks nasty, but it is the most reliant way to get this right.

            try
            {
                return Path.GetFullPath(path);
            }
            catch (Exception ex) when (
                   ex is ArgumentException
                || ex is NotSupportedException
                || ex is PathTooLongException
            )
            {
                throw new ArgumentException(
                    ExceptionStrings.Path.InvalidFormat(),
                    nameof(path),
                    ex
                );
            }
        }

        public override StoragePath Append(string part)
        {
            _ = part ?? throw new ArgumentNullException(nameof(part));
            if (part.Length == 0)
            {
                return this;
            }

            return FileSystem.GetPath(ToString() + part);
        }

        public override StoragePath Combine(string other)
        {
            _ = other ?? throw new ArgumentNullException(nameof(other));
            if (other.Length == 0)
            {
                return this;
            }

            return FileSystem.GetPath(Path.Combine(ToString(), other));
        }

        public override StoragePath Join(string other)
        {
            _ = other ?? throw new ArgumentNullException(nameof(other));
            if (other.Length == 0)
            {
                return this;
            }

            return FileSystem.GetPath(Path.Join(ToString(), other));
        }

        public override StoragePath TrimEndingDirectorySeparator() =>
            _pathWithoutEndingDirectorySeparatorLazy.Value;

    }

}
