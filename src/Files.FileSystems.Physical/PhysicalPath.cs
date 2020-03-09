namespace Files.FileSystems.Physical
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Files;
    using Files.FileSystems.Physical.Resources;
    using Files.FileSystems.Physical.Utilities;
    using IOPath = System.IO.Path;
    using Path = Files.Path;

    internal sealed class PhysicalPath : Path
    {

        private readonly Lazy<Path?> _rootLazy;
        private readonly Lazy<Path?> _parentLazy;
        private readonly Lazy<Path> _fullPathLazy;
        private readonly Lazy<Path> _pathWithoutEndingDirectorySeparatorLazy;

        public override FileSystem FileSystem { get; }

        public override PathKind Kind { get; }

        public override Path? Root => _rootLazy.Value;

        public override Path? Parent => _parentLazy.Value;

        public override Path FullPath => _fullPathLazy.Value;

        public override string Name { get; }

        public override string NameWithoutExtension { get; }

        public override string? Extension { get; }

        public override bool EndsInDirectorySeparator { get; }

        internal PhysicalPath(PhysicalFileSystem fileSystem, string path)
            : base(path)
        {
            var fullPath = GetFullPathOrThrow(path);
            var rootPath = IOPath.GetPathRoot(ToString());
            var pathWithoutTrailingSeparator = IOPath.TrimEndingDirectorySeparator(path);
            var directoryPath = IOPath.GetDirectoryName(pathWithoutTrailingSeparator);
            var name = IOPath.GetFileName(pathWithoutTrailingSeparator);
            var nameWithoutExtension = IOPath.GetFileNameWithoutExtension(pathWithoutTrailingSeparator);
            var extension = PathHelper.GetExtensionWithoutTrailingExtensionSeparator(pathWithoutTrailingSeparator);
            var isPathFullyQualified = IOPath.IsPathFullyQualified(path);
            var endsInDirectorySeparator = IOPath.EndsInDirectorySeparator(path);

            FileSystem = fileSystem;
            Kind = isPathFullyQualified ? PathKind.Absolute : PathKind.Relative;
            Name = name;
            NameWithoutExtension = nameWithoutExtension;
            Extension = string.IsNullOrEmpty(extension) ? null : extension;
            EndsInDirectorySeparator = endsInDirectorySeparator;

            _rootLazy = new Lazy<Path?>(
                () => string.IsNullOrEmpty(rootPath) ? null : fileSystem.GetPath(rootPath),
                isThreadSafe: false
            );

            _parentLazy = new Lazy<Path?>(
                () => string.IsNullOrEmpty(directoryPath) ? null : fileSystem.GetPath(directoryPath),
                isThreadSafe: false
            );

            _fullPathLazy = new Lazy<Path>(
                () => fileSystem.GetPath(fullPath),
                isThreadSafe: false
            );

            _pathWithoutEndingDirectorySeparatorLazy = new Lazy<Path>(
                () =>
                {
                    if (!EndsInDirectorySeparator)
                    {
                        return this;
                    }

                    var trimmedPath = IOPath.TrimEndingDirectorySeparator(ToString());

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
                },
                isThreadSafe: false
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
                return IOPath.GetFullPath(path);
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

        public override Path Append(string part)
        {
            _ = part ?? throw new ArgumentNullException(nameof(part));
            if (part.Length == 0)
            {
                return this;
            }

            return FileSystem.GetPath(ToString() + part);
        }

        public override Path Combine(string other)
        {
            _ = other ?? throw new ArgumentNullException(nameof(other));
            if (other.Length == 0)
            {
                return this;
            }

            return FileSystem.GetPath(IOPath.Combine(ToString(), other));
        }

        public override Path Join(string other)
        {
            _ = other ?? throw new ArgumentNullException(nameof(other));
            if (other.Length == 0)
            {
                return this;
            }

            return FileSystem.GetPath(IOPath.Join(ToString(), other));
        }

        public override Path TrimEndingDirectorySeparator() =>
            _pathWithoutEndingDirectorySeparatorLazy.Value;

    }

}
