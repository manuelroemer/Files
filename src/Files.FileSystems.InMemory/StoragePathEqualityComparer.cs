namespace Files.FileSystems.InMemory
{
    using System.Collections.Generic;
    using Files;

    public class StoragePathEqualityComparer : EqualityComparer<StoragePath?>
    {
        public static new StoragePathEqualityComparer Default { get; } = new StoragePathEqualityComparer();

        public sealed override bool Equals(StoragePath? path1, StoragePath? path2)
        {
            if (path1 is null && path2 is null)
            {
                return true;
            }

            if (path1 is object && path2 is object)
            {
                return EqualsCore(path1.FullPath, path2.FullPath);
            }

            return false;
        }

        protected virtual bool EqualsCore(StoragePath fullPath1, StoragePath fullPath2)
        {
            return fullPath1 == fullPath2;
        }
        
        public sealed override int GetHashCode(StoragePath? path)
        {
            return path is null ? 0 : GetHashCodeCore(path.FullPath);
        }

        protected virtual int GetHashCodeCore(StoragePath fullPath)
        {
            return fullPath.GetHashCode();
        }
    }
}
