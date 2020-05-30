namespace Files.Shared.PhysicalStoragePath
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Files;
    using Files.Shared;
    using Files.Shared.PhysicalStoragePath.Utilities;

    internal sealed class PhysicalStoragePath : StoragePath
    {
        // All properties which return a StoragePath are wrapped in a Lazy<T> in order to not fully
        // expand/walk a path tree whenever a path is initialized.
        // Consider for example the path foo/bar/baz.
        // Without Lazy<T>, three path instances would be created immediately through the Parent property.
        private readonly Lazy<StoragePath?> _rootLazy;
        private readonly Lazy<StoragePath?> _parentLazy;
        private readonly Lazy<StoragePath> _fullPathLazy;

        public override PathKind Kind { get; }

        public override StoragePath? Root => _rootLazy.Value;

        public override StoragePath? Parent => _parentLazy.Value;

        public override StoragePath FullPath => _fullPathLazy.Value;

        public override string Name { get; }

        public override string NameWithoutExtension { get; }

        public override string? Extension { get; }

        internal PhysicalStoragePath(string path, FileSystem fileSystem)
            : base(fileSystem, path)
        {
            Debug.Assert(
                ReferenceEquals(fileSystem.PathInformation, PhysicalPathHelper.PhysicalPathInformation),
                $"When using the PhysicalStoragePath, your file system should be using the corresponding " +
                $"{nameof(PhysicalPathHelper.PhysicalPathInformation)}."
            );

            var fullPath = GetFullPathOrThrow(path);
            var rootPath = Path.GetPathRoot(path);
            var pathWithoutTrailingSeparator = PathPolyfills.TrimEndingDirectorySeparator(path);
            var directoryPath = Path.GetDirectoryName(pathWithoutTrailingSeparator);
            var name = Path.GetFileName(pathWithoutTrailingSeparator);
            var nameWithoutExtension = GetNameWithoutExtension(name);
            var extension = PhysicalPathHelper.GetExtensionWithoutTrailingExtensionSeparator(pathWithoutTrailingSeparator);
            var isPathFullyQualified = PathPolyfills.IsPathFullyQualified(path);

            Kind = isPathFullyQualified ? PathKind.Absolute : PathKind.Relative;
            Name = name;
            NameWithoutExtension = nameWithoutExtension;
            Extension = string.IsNullOrEmpty(extension) ? null : extension;

            _rootLazy = new Lazy<StoragePath?>(
                () => string.IsNullOrEmpty(rootPath) ? null : fileSystem.GetPath(rootPath)
            );

            _parentLazy = new Lazy<StoragePath?>(
                () => string.IsNullOrEmpty(directoryPath) ? null : fileSystem.GetPath(directoryPath)
            );

            _fullPathLazy = new Lazy<StoragePath>(
                () => fileSystem.GetPath(fullPath)
            );

            static string GetFullPathOrThrow(string path)
            {
                // This method, apart from returning the full path, has another goal:
                // Validate that the path has the correct format.
                // Path rules are incredibly complex depending on the current OS. It's best to not try
                // and emulate these rules here, but to actually use the OS/FS APIs, i.e. GetFullPath, directly.
                // One thing to note is that there are differences between the different .NET runtimes here.
                // Older versions have stricter path validations and throw ArgumentExceptions here while
                // newer implementations delay that and throw IOExceptions for invalid paths in the various
                // File/Directory APIs.
                // There is not much we can do about this. Again, emulating and artifically supporting this is
                // incredibly complex. Therefore we simply allow this and document this fact.

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
                        ExceptionStrings.StoragePath.InvalidFormat(),
                        nameof(path),
                        ex
                    );
                }
            }

            static string GetNameWithoutExtension(string name)
            {
                // Specification requires special handling for these two directories.
                // Without this code, we'd return "" and ".", because Path.GetFileNameWithoutExtension
                // trims one dot.
                if (name == PhysicalPathHelper.CurrentDirectorySegment ||
                    name == PhysicalPathHelper.ParentDirectorySegment)
                {
                    return name;
                }
                return Path.GetFileNameWithoutExtension(name);
            }
        }
    }
}
