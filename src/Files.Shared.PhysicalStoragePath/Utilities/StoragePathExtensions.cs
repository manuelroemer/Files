namespace Files.Shared.PhysicalStoragePath.Utilities
{
    using System;
    using Files.Shared.PhysicalStoragePath;

    internal static class StoragePathExtensions
    {
        /// <summary>
        ///     Converts the specified <paramref name="path"/> to a <see cref="PhysicalStoragePath"/>
        ///     which uses the exact <paramref name="fileSystem"/> instance and thereby ensures that:
        ///     
        ///     1. It contains no characters which are illegal in <see cref="PhysicalStoragePath"/>.
        ///     2. All properties and methods of the path do the expected operations (i.e. those of
        ///        the <see cref="PhysicalStoragePath"/>).
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static PhysicalStoragePath ToPhysicalStoragePath(this StoragePath path, FileSystem fileSystem)
        {
            if (path is PhysicalStoragePath physicalStoragePath &&
                ReferenceEquals(physicalStoragePath.FileSystem, fileSystem))
            {
                return physicalStoragePath;
            }
            return new PhysicalStoragePath(path.ToString(), fileSystem);
        }
    }
}
