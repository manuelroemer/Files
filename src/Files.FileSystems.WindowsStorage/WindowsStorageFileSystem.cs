namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Files.Shared.PhysicalStoragePath.Utilities;

    public sealed class WindowsStorageFileSystem : FileSystem
    {
        /// <inheritdoc/>
        public override PathInformation PathInformation => PhysicalPathHelper.PhysicalPathInformation;

        /// <inheritdoc/>
        public override StorageFile GetFile(StoragePath path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override StorageFolder GetFolder(StoragePath path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override StoragePath GetPath(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool TryGetPath(KnownFolder knownFolder, [NotNullWhenAttribute(true)] out StoragePath? result)
        {
            throw new NotImplementedException();
        }
    }
}
