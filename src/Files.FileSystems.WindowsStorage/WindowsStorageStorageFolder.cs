namespace Files.FileSystems.WindowsStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.WindowsStorage.Utilities;
    using Files.Shared.PhysicalStoragePath;
    using WinStorageFile = Windows.Storage.StorageFile;
    using WinStorageFolder = Windows.Storage.StorageFolder;
    using WinCreationCollisionOption = Windows.Storage.CreationCollisionOption;

    internal sealed class WindowsStorageStorageFolder : StorageFolder
    {
        private readonly StoragePath _path;
        private readonly StoragePath _fullPath;
        private readonly StoragePath? _fullParentPath;

        public override FileSystem FileSystem { get; }

        public override StoragePath Path => _path;

        public WindowsStorageStorageFolder(FileSystem fileSystem, PhysicalStoragePath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            FileSystem = fileSystem;
            _path = path;
            _fullPath = Path.FullPath;
            _fullParentPath = _fullPath.Parent;
        }

        public override Task<StorageFolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotImplementedException();
            //// We cannot reasonably create a root directory with the API.
            //// If someone tries to do so, we'll simply deny the call. In most cases, the root
            //// folder will exist anyway.
            //if (_fullParentPath is null)
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //if (recursive)
            //{

            //}
            //else
            //{
            //    var parent = await WinStorageFolder
            //        .GetFolderFromPathAsync(_fullParentPath.ToString())
            //        .Cancel(cancellationToken);

            //    await parent
            //        .CreateFolderAsync(_path.Name, options.ToWinOptions())
            //        .Cancel(cancellationToken);
            //}

            //async Task RecursiveImpl(StoragePath? currentParentPath, StoragePath? previousParentPath)
            //{
            //    if (currentParentPath is null)
            //    {
            //        // Reached root folder.
            //        throw new UnauthorizedAccessException();
            //    }

            //    if (previousParentPath is null)
            //    {
            //        // Reached this folder.
            //        return;
            //    }

            //    try
            //    {
            //        var parent = await WinStorageFolder
            //            .GetFolderFromPathAsync(currentParentPath.ToString())
            //            .Cancel(cancellationToken);

            //        // Without an exception, the folder exists. We can now go up and, step by step,
            //        // create the missing folders in the chain.
            //        await parent
            //            .CreateFolderAsync(previousParentPath.Name, WinCreationCollisionOption.OpenIfExists)
            //            .Cancel(cancellationToken);
            //    }
            //    catch (FileNotFoundException)
            //    {
            //        await RecursiveImpl(currentParentPath.Parent, currentParentPath).ConfigureAwait(false);
            //    }
            //}
        }

        public override Task<StorageFolder> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            throw new NotImplementedException();
        }

        public override Task<StorageFolder> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            throw new NotImplementedException();
        }

        public override Task<StorageFolder> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<StorageFile>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<StorageFolder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
