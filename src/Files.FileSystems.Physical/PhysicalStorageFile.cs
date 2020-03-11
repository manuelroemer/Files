namespace Files.FileSystems.Physical
{
    using System;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.Physical.Resources;
    using Files.FileSystems.Physical.Utilities;
    using Files.Utilities;
    using IOPath = System.IO.Path;

    internal sealed class PhysicalStorageFile : StorageFile
    {

        private readonly StoragePath _path;
        private readonly StoragePath _fullPath;
        private readonly StoragePath _fullParentPath;
        private readonly FileInfo _fileInfo;

        public override FileSystem FileSystem { get; }

        public override StoragePath Path => _path;

        public PhysicalStorageFile(FileSystem fileSystem, PhysicalStoragePath path)
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

        public override Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                _fileInfo.Refresh();
                EnsureExists(cancellationToken);

                // Attempting to get the real file name can fail, e.g. the file might have been deleted in between.
                // In such a case, simply return the last fetched name. It will happen rarely and is good enough
                // for such cases.
                cancellationToken.ThrowIfCancellationRequested();
                var realFileName = _fileInfo.GetRealName() ?? _fileInfo.Name;
                var lastWriteTime = File.GetLastWriteTimeUtc(_fullPath.ToString());

                return new StorageFileProperties(
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
            return Task.Run(() =>
            {
                EnsureNoConflictingFolderExists();
                cancellationToken.ThrowIfCancellationRequested();
                return File.GetAttributes(_fullPath.ToString());
            });
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                EnsureNoConflictingFolderExists();
                cancellationToken.ThrowIfCancellationRequested();
                File.SetAttributes(_fullPath.ToString(), attributes);
            });
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() => File.Exists(_fullPath.ToString()));
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

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    new FileStream(_fullPath.ToString(), options.ToFileMode()).Dispose();
                }
                catch (UnauthorizedAccessException ex)
                {
                    RethrowUnauthorizedAccessExceptionAsIOExceptionOnConflictingFolder(ex);
                }
            });
        }

        public override Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var overwrite = options.ToOverwriteBool();
                File.Copy(_fullPath.ToString(), destinationPath.FullPath.ToString(), overwrite);
                return FileSystem.GetFile(destinationPath);
            });
        }

        public override Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var overwrite = options.ToOverwriteBool();
                File.Move(_fullPath.ToString(), destinationPath.FullPath.ToString(), overwrite);
                return FileSystem.GetFile(destinationPath);
            });
        }

        public override Task<StorageFile> RenameAsync(
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
            return Task.Run(() =>
            {
                if (options == DeletionOption.Fail)
                {
                    EnsureExists(cancellationToken);
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    File.Delete(_fullPath.ToString());
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                {
                    // The exception is thrown if a parent directory does not exist.
                    // Must be caught manually to ensure compatibility with DeletionOption.IgnoreMissing.
                    if (options != DeletionOption.IgnoreMissing)
                    {
                        throw;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    RethrowUnauthorizedAccessExceptionAsIOExceptionOnConflictingFolder(ex);
                }
            });
        }

        public override Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run<Stream>(() => File.Open(_fullPath.ToString(), FileMode.Open, fileAccess));
        }

        public override Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            return File.ReadAllBytesAsync(_fullPath.ToString(), cancellationToken);
        }

        public override Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            return encoding is null
                ? File.ReadAllTextAsync(_fullPath.ToString(), cancellationToken)
                : File.ReadAllTextAsync(_fullPath.ToString(), encoding, cancellationToken);
        }

        public override async Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await File.WriteAllBytesAsync(_fullPath.ToString(), bytes, cancellationToken).ConfigureAwait(false);
        }

        public override async Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        )
        {
            _ = text ?? throw new ArgumentNullException(nameof(text));
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            if (encoding is null)
            {
                await File.WriteAllTextAsync(_fullPath.ToString(), text, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await File.WriteAllTextAsync(_fullPath.ToString(), text, encoding, cancellationToken).ConfigureAwait(false);
            }
        }

        private Task EnsureExistsAsync(CancellationToken cancellationToken) =>
            Task.Run(() => EnsureExists(cancellationToken));

        private void EnsureExists(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(_fullParentPath.ToString()))
            {
                throw new DirectoryNotFoundException();
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(_fullPath.ToString()))
            {
                throw new FileNotFoundException();
            }
        }

        private void EnsureNoConflictingFolderExists()
        {
            if (Directory.Exists(_fullPath.ToString()))
            {
                throw new IOException(ExceptionStrings.File.ConflictingFolderExistsAtFileLocation());
            }
        }

        /// <summary>
        ///     Several methods in the <see cref="File"/> class throw an <see cref="UnauthorizedAccessException"/>
        ///     when a file operation (e.g. Create, Delete, ...) is executed on a directory.
        ///     
        ///     Per library specification, the library should throw an IOException in such cases.
        ///     To do this, we manually check if a directory exists in such a location.
        ///     If so, we certainly have access to the location and can throw the appropriate exception.
        /// </summary>
        private void RethrowUnauthorizedAccessExceptionAsIOExceptionOnConflictingFolder(UnauthorizedAccessException originalException)
        {
            bool hasConflictingDirectory;

            try
            {
                hasConflictingDirectory = Directory.Exists(_fullPath.ToString());
            }
            // Do not catch general exception types
            // Since this is only used for exception conversions, it's okay to fail.
#pragma warning disable CA1031
            catch
            {
                hasConflictingDirectory = false;
            }
#pragma warning restore CA1031

            if (hasConflictingDirectory)
            {
                throw new IOException(ExceptionStrings.File.ConflictingFolderExistsAtFileLocation(), originalException);
            }
            else
            {
                ExceptionDispatchInfo.Throw(originalException);
            }
        }

    }

}
