namespace Files.FileSystems.Physical
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.Physical.Resources;
    using Files.FileSystems.Physical.Utilities;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
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
                var realFileName = FsHelper.GetRealFileName(_fullPath.ToString()) ?? _fileInfo.Name;
                var lastWriteTime = File.GetLastWriteTimeUtc(_fullPath.ToString());

                return new StorageFileProperties(
                    realFileName,
                    IOPath.GetFileNameWithoutExtension(realFileName),
                    PhysicalPathHelper.GetExtensionWithoutTrailingExtensionSeparator(realFileName)?.ToNullIfEmpty(),
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
                EnsureNoConflictingFolderExists(_fullPath.ToString());
                cancellationToken.ThrowIfCancellationRequested();
                return File.GetAttributes(_fullPath.ToString());
            });
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                EnsureNoConflictingFolderExists(_fullPath.ToString());
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
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    throw;
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
                var dstPathString = destinationPath.FullPath.ToString();
                var overwrite = options.ToOverwriteBool();

#if NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2
                // File.Copy on Unix systems AND .NET Core 2.0 - 2.2 has this weird behavior where
                // no exception is thrown if the destination is a folder. When copying a file to
                // a conflicting folder, the API, instead of throwing, simply moves the file *into*
                // the folder. For example, assume that we copy "src/srcFile.ext" to "dst":
                // |_ src
                // |  |_ srcFile.ext
                // |_ dst
                //
                // We'd assume that this throws, but instead, this happens:
                // |_ src
                // |  |_ srcFile.ext
                // |_ dst
                //    |_ srcFile.ext
                // 
                // This can be fixed by preemptively verifying that there is no conflicting folder.
                // This has the disadvantage that we lose the inner exception which would normally
                // be thrown (the UnauthorizedAccessException below).
                // To not lose it with other TFMs, only include it in the failing .NET Core versions.
                EnsureNoConflictingFolderExists(_fullPath.ToString());
#endif

                try
                {
                    File.Copy(_fullPath.ToString(), dstPathString, overwrite);
                    return FileSystem.GetFile(destinationPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    EnsureNoConflictingFolderExists(dstPathString, ex);
                    throw;
                }
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
                EnsureExists(cancellationToken);

                var fullDestinationPath = destinationPath.FullPath;
                var overwrite = options.ToOverwriteBool();

                // System.IO doesn't throw when moving files to the same location.
                // Detecting this via paths will not always work, but it fulfills the spec most of the time.
                if (_fullPath == fullDestinationPath)
                {
                    throw new IOException(ExceptionStrings.File.CannotMoveToSameLocation());
                }

                try
                {
                    FilePolyfills.Move(_fullPath.ToString(), fullDestinationPath.ToString(), overwrite);
                    return FileSystem.GetFile(destinationPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    EnsureNoConflictingFolderExists(fullDestinationPath.ToString(), ex);
                    throw;
                }
            });
        }

        public override Task<StorageFile> RenameAsync(
            string newName,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            if (newName.Length == 0)
            {
                throw new ArgumentException(ExceptionStrings.String.CannotBeEmpty(), nameof(newName));
            }

            if (newName.Contains(PhysicalPathHelper.InvalidNewNameCharacters))
            {
                throw new ArgumentException(
                    ExceptionStrings.File.NewNameContainsInvalidChar(FileSystem.PathInformation),
                    nameof(newName)
                );
            }

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
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    throw;
                }
            });
        }

        public override Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run<Stream>(() =>
            {
                try
                {
                    return File.Open(_fullPath.ToString(), FileMode.Open, fileAccess);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    throw;
                }
            });
        }

        public override Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                try
                {
                    return await FilePolyfills.ReadAllBytesAsync(_fullPath.ToString(), cancellationToken).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    throw;
                }
            });
        }

        public override Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));

            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                EnsureExists(cancellationToken);
                await FilePolyfills.WriteAllBytesAsync(_fullPath.ToString(), bytes, cancellationToken).ConfigureAwait(false);
            });
        }

        public override Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                try
                {
                    return encoding is null
                        ? await FilePolyfills.ReadAllTextAsync(_fullPath.ToString(), cancellationToken).ConfigureAwait(false)
                        : await FilePolyfills.ReadAllTextAsync(_fullPath.ToString(), encoding, cancellationToken).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    throw;
                }
            });
        }

        public override Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        )
        {
            _ = text ?? throw new ArgumentNullException(nameof(text));

            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                EnsureExists(cancellationToken);

                if (encoding is null)
                {
                    await FilePolyfills.WriteAllTextAsync(_fullPath.ToString(), text, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await FilePolyfills.WriteAllTextAsync(_fullPath.ToString(), text, encoding, cancellationToken).ConfigureAwait(false);
                }
            });
        }

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

        private void EnsureNoConflictingFolderExists(string path, Exception? innerException = null)
        {
            if (Directory.Exists(path))
            {
                throw new IOException(ExceptionStrings.File.ConflictingFolderExistsAtFileLocation(), innerException);
            }
        }
    }
}
