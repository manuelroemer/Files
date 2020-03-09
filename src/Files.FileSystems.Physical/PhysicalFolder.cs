namespace Files.FileSystems.Physical
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.Physical.Resources;
    using Files.FileSystems.Physical.Utilities;
    using Files.Utilities;
    using File = File;
    using IOFile = System.IO.File;
    using IOPath = System.IO.Path;
    using Path = Path;

    internal sealed class PhysicalFolder : Folder
    {

        private readonly Path _path;
        private readonly Path _fullPath;
        private readonly Path? _fullParentPath;
        private readonly DirectoryInfo _directoryInfo;

        public override FileSystem FileSystem { get; }

        public override Path Path => _path;

        public PhysicalFolder(FileSystem fileSystem, PhysicalPath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            FileSystem = fileSystem;
            _path = path;
            _fullPath = path.FullPath;
            _fullParentPath = path.FullPath.Parent;
            _directoryInfo = new DirectoryInfo(_fullPath);
        }

        public override Task<FolderProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                _directoryInfo.Refresh();
                await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);

                // Attempting to get the real file name can fail, e.g. the file might have been deleted in between.
                // In such a case, simply return the last fetched name. It will happen rarely and is good enough
                // for such cases.
                cancellationToken.ThrowIfCancellationRequested();
                var realFileName = _directoryInfo.GetRealName() ?? _directoryInfo.Name;
                var lastWriteTime = Directory.GetLastWriteTimeUtc(_fullPath.ToString());

                return new FolderProperties(
                    realFileName,
                    IOPath.GetFileNameWithoutExtension(realFileName),
                    PathHelper.GetExtensionWithoutTrailingExtensionSeparator(realFileName)?.ToNullIfEmpty(),
                    _directoryInfo.CreationTimeUtc,
                    lastWriteTime
                );
            });
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() => IOFile.GetAttributes(_fullPath.ToString()));
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() => IOFile.SetAttributes(_fullPath.ToString(), attributes));
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() => Directory.Exists(_fullPath.ToString()));
        }

        public override Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                // Directory.CreateDirectory is recursive by default.
                // If recursive is false, we must manually ensure that the parent directory exists (if this is not
                // a root directory).
                if (!recursive && _fullParentPath is object && !Directory.Exists(_fullParentPath.ToString()))
                {
                    throw new DirectoryNotFoundException();
                }

                // Directory.CreateDirectory is very user friendly in the sense that it rarely throws exceptions.
                // This means that we have to manually implement support for collision options.
                // This can lead to race conditions, but it's the best we can do.
                if (await ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    switch (options)
                    {
                        case CreationCollisionOption.Fail:
                            throw new IOException(ExceptionStrings.Folder.CreateFailFolderAlreadyExists());
                        case CreationCollisionOption.ReplaceExisting:
                            await DeleteAsync(DeletionOption.IgnoreMissing).ConfigureAwait(false);
                            break;
                        case CreationCollisionOption.Ignore:
                            return;
                        default:
                            throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options));
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(_fullPath.ToString());
            });
        }

        public override Task<Folder> CopyAsync(
            Path destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(async () =>
            {
                var destination = FileSystem.GetFolder(destinationPath);
                var overwrite = options.ToOverwriteBool();

                if (overwrite && await destination.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    await destination.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken).ConfigureAwait(false);
                }

                DirectoryHelper.CopyDirectory(_fullPath.ToString(), destinationPath.FullPath.ToString(), cancellationToken);
                return destination;
            });
        }

        public override Task<Folder> MoveAsync(
            Path destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(async () =>
            {
                var destination = FileSystem.GetFolder(destinationPath);
                var overwrite = options.ToOverwriteBool();

                if (overwrite && await destination.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    await destination.DeleteAsync(DeletionOption.IgnoreMissing, cancellationToken).ConfigureAwait(false);
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.Move(_fullPath.ToString(), destinationPath.FullPath.ToString());
                return destination;
            });
        }

        public override Task<Folder> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            cancellationToken.ThrowIfCancellationRequested();
            var destinationPath = _fullParentPath?.Join(newName) ?? FileSystem.GetPath(newName);
            return MoveAsync(destinationPath, options, cancellationToken);
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                if (options == DeletionOption.Fail)
                {
                    await EnsureExistsAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.Delete(_fullPath.ToString(), recursive: true);
            });
        }

        public override Task<IEnumerable<File>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                return Directory
                    .GetFiles(_fullPath.ToString())
                    .Select(path => FileSystem.GetFile(path));
            });
        }

        public override Task<IEnumerable<Folder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                return Directory
                    .GetDirectories(_fullPath.ToString())
                    .Select(path => FileSystem.GetFolder(path));
            });
        }

        private async ValueTask EnsureExistsAsync(CancellationToken cancellationToken)
        {
            if (!await ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new DirectoryNotFoundException();
            }
        }

    }

}
