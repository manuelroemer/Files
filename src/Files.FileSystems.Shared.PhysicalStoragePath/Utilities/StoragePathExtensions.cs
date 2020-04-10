namespace Files.FileSystems.Shared.PhysicalStoragePath.Utilities
{
    using System;
    using Files.FileSystems.Shared.PhysicalStoragePath;

    internal static class StoragePathExtensions
    {
        /// <summary>
        ///     Converts the specified <paramref name="path"/> to a <see cref="PhysicalStoragePath"/>
        ///     which uses the exact <paramref name="fileSystem"/> instance and thereby ensures that
        ///     it contains no characters which are illegal in <see cref="PhysicalStoragePath"/>.
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
