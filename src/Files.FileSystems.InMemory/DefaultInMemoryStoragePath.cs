namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Files;
    using Files.Shared;

    /// <summary>
    ///     See <see cref="DefaultInMemoryStoragePathProvider"/> for a detailed description about
    ///     the path's behavior.
    ///     
    ///     In summary, this is a path implementation inspired by Unix, but slimmed down.
    ///     The class is entirely based on a <see cref="PathInformation"/> value, i.e. every single
    ///     character (e.g. '/' for separators or '.' for extension separators) can be swapped out.
    /// </summary>
    internal sealed class DefaultInMemoryStoragePath : StoragePath
    {
        private readonly PathInformation _pathInformation;
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
        
        public DefaultInMemoryStoragePath(InMemoryFileSystem fileSystem, string path)
            : base(fileSystem, path)
        {
            _pathInformation = fileSystem.PathInformation;

            if (path.Contains(_pathInformation.InvalidPathChars.ToArray()) ||
                path.Contains(_pathInformation.InvalidFileNameChars.ToArray()))
            {
                throw new ArgumentException(ExceptionStrings.StoragePath.InvalidFormat(), nameof(path));
            }

            Kind = GetKind();
            Name = GetName();
            NameWithoutExtension = GetNameWithoutExtension();
            Extension = GetExtension();
            _rootLazy = new Lazy<StoragePath?>(() => GetRoot());
            _parentLazy = new Lazy<StoragePath?>(() => GetParent());
            _fullPathLazy = new Lazy<StoragePath>(() => GetFullPath());
        }

        private PathKind GetKind()
        {
            // Following Unix, a rooted path is treated as an absolute path.
            return IsRooted() ? PathKind.Absolute : PathKind.Relative;
        }

        private StoragePath? GetRoot()
        {
            if (!IsRooted())
            {
                return null;
            }

            return FileSystem.GetPath(_pathInformation.DirectorySeparatorChar.ToString());
        }

        private StoragePath? GetParent()
        {
            if (IsRootPathOnly())
            {
                return null;
            }

            // Find the index of the first separator from behind and then (to fulfill spec) continue
            // walking down until the last consecutive char has been passed.
            var rootLength = GetRootLength();
            var endIndex = Length;

            while (endIndex > rootLength && !IsDirectorySeparator(UnderlyingString[endIndex - 1]))
            {
                endIndex--;
            }

            while (endIndex > rootLength && IsDirectorySeparator(UnderlyingString[endIndex - 1]))
            {
                endIndex--;
            }

            // If there has been no directory separator there is no parent (can happen with relative paths).
            if (endIndex <= 0)
            {
                return null;
            }

            return FileSystem.GetPath(UnderlyingString.Substring(0, endIndex));
        }

        private StoragePath GetFullPath()
        {
            // While it's common in .NET to append the user/CWD to the beginning of an un-rooted path,
            // we cannot do this here, because we don't have any information about such a location.
            // The next best thing is to just append a directory separator in front.
            // Apart from that, any relative segments and consecutive separators are removed here,
            // resulting in a full path.
            var path = UnderlyingString;
            path = RemoveRelativeSegments(path);
            path = RemoveConsecutiveSeparators(path);
            path = MakeRoot(path);
            return FileSystem.GetPath(path);

            string RemoveRelativeSegments(string path)
            {
                var segments = path.Split(_pathInformation.DirectorySeparatorChars.ToArray());
                var resolvedSegments = new List<string>(segments.Length);
                // A Stack would be better than a List here, but it can only be iterated from "behind".
                // This is bad because the resolved segments must be joined from the start.
                // Therefore a List is the better choice.

                foreach (var segment in segments)
                {
                    if (IsCurrentDirectorySegment(segment))
                    {
                        // The same directory (e.g. ".") is just skipped.
                        continue;
                    }
                    else if (IsParentDirectorySegment(segment) && resolvedSegments.Count > 0)
                    {
                        // A parent directory (e.g. "..") leads to the removal of the last element.
                        resolvedSegments.RemoveAt(resolvedSegments.Count - 1);
                    }
                    else
                    {
                        resolvedSegments.Add(segment);
                    }
                }

                return string.Join(_pathInformation.DirectorySeparatorChar.ToString(), resolvedSegments);
            }

            string RemoveConsecutiveSeparators(string path)
            {
                // Do two passes to potentially skip a StringBuilder allocation (similar to .NET's implementation).
                return ShouldRemove() ? Remove() : path;

                bool ShouldRemove()
                {
                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        if (IsDirectorySeparator(path[i]) && IsDirectorySeparator(path[i + 1]))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                string Remove()
                {
                    var sb = new StringBuilder(path.Length);

                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        // Skip two consecutive separators.
                        if (IsDirectorySeparator(path[i]) && IsDirectorySeparator(path[i + 1]))
                        {
                            continue;
                        }

                        sb.Append(path[i]);
                    }

                    return sb.ToString();
                }
            }

            string MakeRoot(string path)
            {
                if (path.Length == 0 || !_pathInformation.DirectorySeparatorChars.Contains(path[0]))
                {
                    return $"{_pathInformation.DirectorySeparatorChar}{path}";
                }
                return path;
            }
        }

        private string GetName()
        {
            if (IsRootPathOnly())
            {
                return string.Empty;
            }

            // Specification requires us to ignore exactly one trailing directory separator character.
            // The name starts at the position of the last directory separator character except for
            // that trailing one.
            var startIndex = 0;
            var endIndex = EndsWithDirectorySeparator ? Length - 2 : Length - 1;
            
            for (var i = endIndex; i >= 0; i--)
            {
                if (IsDirectorySeparator(UnderlyingString[i]))
                {
                    startIndex = i + 1;
                    break;
                }
            }

            return UnderlyingString.Substring(startIndex, endIndex - startIndex + 1);
        }

        private string GetNameWithoutExtension()
        {
            var name = GetName();

            // Specification requires special handling for Current/Parent segments.
            if (IsCurrentDirectorySegment(name) || IsParentDirectorySegment(name))
            {
                return name;
            }

            // Otherwise, just split at the last extension separator.
            var lastExtensionSeparatorIndex = name.LastIndexOf(_pathInformation.ExtensionSeparatorChar);
            return lastExtensionSeparatorIndex < 0
                ? name
                : name.Substring(0, lastExtensionSeparatorIndex);
        }

        private string? GetExtension()
        {
            var name = GetName();

            for (var i = name.Length - 1; i >= 0; i--)
            {
                if (name[i] == _pathInformation.ExtensionSeparatorChar)
                {
                    return name.Substring(i + 1).ToNullIfEmpty();
                }
            }

            return null;
        }

        private bool IsRooted() =>
            IsDirectorySeparator(UnderlyingString[0]);

        /// <summary>Returns whether the path consists of a single directory separator, i.e. if it's rooted.</summary>
        private bool IsRootPathOnly() =>
            IsRooted() && Length == 1;

        private int GetRootLength() =>
            IsRooted() ? 1 : 0;

        private bool IsDirectorySeparator(char c) =>
            _pathInformation.DirectorySeparatorChars.Contains(c);

        private bool IsCurrentDirectorySegment(string s) =>
            s.Equals(_pathInformation.CurrentDirectorySegment, _pathInformation.DefaultStringComparison);

        private bool IsParentDirectorySegment(string s) =>
            s.Equals(_pathInformation.ParentDirectorySegment, _pathInformation.DefaultStringComparison);
    }
}
