namespace Files.FileSystems.Physical
{
    using System;
    using System.IO;
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

    internal sealed class PhysicalFile : File
    {

        private readonly Path _path;
        private readonly Path _fullPath;
        private readonly Path _fullParentPath;
        private readonly FileInfo _fileInfo;

        public override FileSystem FileSystem { get; }

        public override Path Path => _path;

        public PhysicalFile(FileSystem fileSystem, PhysicalPath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            if (path.FullPath.Parent is null)
            {
                throw new ArgumentException(
                    ExceptionStrings.File.CannotInitializeWithRootFolderPath(),
                    nameof(path)
                );
            }

            FileSystem = fileSystem;
            _path = path;
            _fullPath = path.FullPath;
            _fullParentPath = path.FullPath.Parent;
            _fileInfo = new FileInfo(_fullPath);
        }

        public override Task<FileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                _fileInfo.Refresh();
                await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);

                // Attempting to get the real file name can fail, e.g. the file might have been deleted in between.
                // In such a case, simply return the last fetched name. It will happen rarely and is good enough
                // for such cases.
                cancellationToken.ThrowIfCancellationRequested();
                var realFileName = _fileInfo.GetRealName() ?? _fileInfo.Name;
                var lastWriteTime = IOFile.GetLastWriteTimeUtc(_fullPath.ToString());

                return new FileProperties(
                    realFileName,
                    IOPath.GetFileNameWithoutExtension(realFileName),
                    PathHelper.GetExtensionWithoutTrailingExtensionSeparator(realFileName)?.ToNullIfEmpty(),
                    _fileInfo.CreationTimeUtc,
                    lastWriteTime,
                    (ulong)_fileInfo.Length
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
            return Task.Run(() => IOFile.Exists(_fullPath.ToString()));
        }

        public override Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                if (recursive)
                {
                    Directory.CreateDirectory(_fullParentPath.ToString());
                }

                cancellationToken.ThrowIfCancellationRequested();
                new FileStream(_fullPath.ToString(), options.ToFileMode()).Dispose();
            });
        }

        public override Task<File> CopyAsync(
            Path destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var overwrite = options.ToOverwriteBool();
                IOFile.Copy(_fullPath.ToString(), destinationPath.FullPath.ToString(), overwrite);
                return FileSystem.GetFile(destinationPath);
            });
        }

        public override Task<File> MoveAsync(
            Path destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var overwrite = options.ToOverwriteBool();
                IOFile.Move(_fullPath.ToString(), destinationPath.FullPath.ToString(), overwrite);
                return FileSystem.GetFile(destinationPath);
            });
        }

        public override Task<File> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            cancellationToken.ThrowIfCancellationRequested();
            var destinationPath = _fullParentPath.Join(newName);
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
                IOFile.Delete(_fullPath.ToString());
            });
        }

        public override Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run<Stream>(() => IOFile.Open(_fullPath.ToString(), FileMode.Open, fileAccess));
        }

        public override Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            return IOFile.ReadAllBytesAsync(_fullPath.ToString(), cancellationToken);
        }

        public override Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            return encoding is null
                ? IOFile.ReadAllTextAsync(_fullPath.ToString(), cancellationToken)
                : IOFile.ReadAllTextAsync(_fullPath.ToString(), encoding, cancellationToken);
        }

        public override Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));
            return IOFile.WriteAllBytesAsync(_fullPath.ToString(), bytes, cancellationToken);
        }

        public override async Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        )
        {
            _ = text ?? throw new ArgumentNullException(nameof(text));
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);

            if (encoding is null)
            {
                await IOFile.WriteAllTextAsync(_fullPath.ToString(), text, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await IOFile.WriteAllTextAsync(_fullPath.ToString(), text, encoding, cancellationToken).ConfigureAwait(false);
            }
        }

        private async ValueTask EnsureExistsAsync(CancellationToken cancellationToken)
        {
            if (!await GetParent().ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new DirectoryNotFoundException();
            }

            if (!await ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new FileNotFoundException();
            }
        }

    }

}
