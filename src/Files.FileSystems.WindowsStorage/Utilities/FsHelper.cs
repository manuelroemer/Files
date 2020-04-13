namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System;
    using System.Threading.Tasks;
    using WinStorageFile = Windows.Storage.StorageFile;
    using WinStorageFolder = Windows.Storage.StorageFolder;
    using WinCreationCollisionOption = Windows.Storage.CreationCollisionOption;
    using System.IO;
    using System.Threading;
    using Files;
    using System.Runtime.ExceptionServices;
    using Files.Shared;

    internal static class FsHelper
    {
        internal static async Task<WinStorageFile> GetFileAsync(
            StoragePath fullPath,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await WinStorageFile.GetFileFromPathAsync(fullPath).AsAwaitable(cancellationToken);
            }
            catch (FileNotFoundException ex)
            {
                // FileNotFoundException might be thrown if the parent directory does not exist.
                // Specification requires DirectoryNotFoundException in such cases.

                // Some folder operations might call this method though. In such cases, we might not
                // have a parent path. In that case, just go with the FileNotFoundException.
                if (fullPath.Parent is null)
                {
                    throw;
                }

                try
                {
                    await WinStorageFolder.GetFolderFromPathAsync(fullPath.Parent).AsAwaitable(cancellationToken);
                }
                catch
                {
                    // Getting the parent folder failed.
                    throw new DirectoryNotFoundException(ExceptionStrings.StorageFile.ParentFolderDoesNotExist(), ex);
                }

                // At this point we know that the parent folder exists, but the file doesn't.
                // We can go with the FileNotFoundException.
                throw;
            }
            catch (ArgumentException ex)
            {
                // An ArgumentException might indicate that a conflicting folder exists at this location.
                // Try to get a folder. If that succeedes, we can be certain and throw the IOException
                // which is required by specification.
                try
                {
                    await WinStorageFolder.GetFolderFromPathAsync(fullPath).AsAwaitable(cancellationToken);
                }
                catch
                {
                    // Getting the folder failed too. Rethrow the ArgumentException from before.
                    ExceptionDispatchInfo.Throw(ex);
                }

                // At this point, getting the folder succeeded. We must manually throw to fulfill the specification.
                throw new IOException(ExceptionStrings.StorageFile.ConflictingFolderExistsAtFileLocation(), ex);
            }
        }

        internal static async Task<WinStorageFolder> GetOrCreateFolderAsync(
            StoragePath fullPath,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // If this succeedes, the folder exists. Nothing to do.
                return await GetFolderAsync(fullPath, cancellationToken).ConfigureAwait(false);
            }
            catch (DirectoryNotFoundException)
            {
                var parentPath = fullPath.Parent;
                if (parentPath is null)
                {
                    // Root folder.
                    throw new UnauthorizedAccessException();
                }

                var parentFolder = await GetOrCreateFolderAsync(parentPath, cancellationToken).ConfigureAwait(false);
                return await parentFolder
                    .CreateFolderAsync(fullPath.Name, WinCreationCollisionOption.OpenIfExists)
                    .AsAwaitable(cancellationToken);
            }
        }

        internal static async Task<WinStorageFolder> GetFolderAsync(
            StoragePath fullPath,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await WinStorageFolder.GetFolderFromPathAsync(fullPath).AsAwaitable(cancellationToken);
            }
            catch (FileNotFoundException ex)
            {
                throw new DirectoryNotFoundException(message: null, innerException: ex);
            }
            catch (ArgumentException ex)
            {
                // An ArgumentException might indicate that a conflicting file exists at this location.
                // Try to get a file. If that succeedes, we can be certain and throw the IOException
                // which is required by specification.
                try
                {
                    await WinStorageFile.GetFileFromPathAsync(fullPath).AsAwaitable(cancellationToken);
                }
                catch
                {
                    // Getting the file failed too. Rethrow the ArgumentException from before.
                    ExceptionDispatchInfo.Throw(ex);
                }

                // At this point, getting the file succeeded. We must manually throw to fulfill the specification.
                throw new IOException(ExceptionStrings.StorageFolder.ConflictingFileExistsAtFolderLocation(), ex);
            }
        }
    }
}
