namespace Files.FileSystems.InMemory
{
    using System.Collections.Generic;
    using Files;

    public class DefaultStoragePathEqualityComparer : EqualityComparer<StoragePath?>
    {
        public static new DefaultStoragePathEqualityComparer Default { get; } = new DefaultStoragePathEqualityComparer();

        public sealed override bool Equals(StoragePath? path1, StoragePath? path2)
        {
            if (path1 is null && path2 is null)
            {
                return true;
            }

            if (path1 is object && path2 is object)
            {
                return EqualsCore(GetFinalPath(path1),  GetFinalPath(path2));
            }

            return false;
        }

        protected virtual bool EqualsCore(StoragePath fullPath1, StoragePath fullPath2)
        {
            return fullPath1 == fullPath2;
        }
        
        public sealed override int GetHashCode(StoragePath? path)
        {
            return path is null ? 0 : GetHashCodeCore(GetFinalPath(path));
        }

        protected virtual int GetHashCodeCore(StoragePath fullPath)
        {
            return fullPath.GetHashCode();
        }

        private static StoragePath GetFinalPath(StoragePath path)
        {
            // Since we don't know the specifics of the underlying path, we've got to settle
            // for the next best comparison possible which is a string-based comparison.
            // Here, always utilize the FullPath and trim unnecessary trailing separators.
            return path.FullPath.TryTrimEndingDirectorySeparator(out var trimmedFullPath)
                ? trimmedFullPath
                : path.FullPath;
        }
    }
}
