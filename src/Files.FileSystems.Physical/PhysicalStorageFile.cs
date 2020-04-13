namespace Files.FileSystems.Physical
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Files;
    using Files.FileSystems.Physical.Utilities;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using Files.Shared;
    using IOPath = System.IO.Path;

    internal sealed class PhysicalStorageFile : StorageFile
    {
        private readonly FileSystem _fileSystem;
        private readonly StoragePath _path;
        private readonly StoragePath _fullPath;
        private readonly StoragePath _fullParentPath;
        private readonly FileInfo _fileInfo;

        public override FileSystem FileSystem => _fileSystem;

        public override StoragePath Path => _path;

        internal PhysicalStorageFile(PhysicalFileSystem fileSystem, PhysicalStoragePath path)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ = path ?? throw new ArgumentNullException(nameof(path));

            if (path.FullPath.Parent is null)
            {
                throw new ArgumentException(
                    ExceptionStrings.StorageFile.CannotInitializeWithRootFolderPath(),
                    nameof(path)
                );
            }

            _fileSystem = fileSystem;
            _path = path;
            _fullPath = path.FullPath;
            _fullParentPath = path.FullPath.Parent;
            _fileInfo = new FileInfo(_fullPath.ToString());
        }

        public override Task<StorageFileProperties> GetPropertiesAsync(CancellationToken cancellationToken = default)
        {
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
            }, cancellationToken);
        }

        public override Task<FileAttributes> GetAttributesAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                EnsureNoConflictingFolderExists(_fullPath.ToString());
                cancellationToken.ThrowIfCancellationRequested();
                return File.GetAttributes(_fullPath.ToString());
            }, cancellationToken);
        }

        public override Task SetAttributesAsync(FileAttributes attributes, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(attributes))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(attributes), nameof(attributes));
            }

            return Task.Run(() =>
            {
                EnsureNoConflictingFolderExists(_fullPath.ToString());
                cancellationToken.ThrowIfCancellationRequested();
                File.SetAttributes(_fullPath.ToString(), attributes);
            }, cancellationToken);
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() => File.Exists(_fullPath.ToString()), cancellationToken);
        }

        public override Task CreateAsync(
            bool recursive,
            CreationCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

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
            }, cancellationToken);
        }

        public override Task<StorageFile> CopyAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            destinationPath = destinationPath.ToPhysicalStoragePath(FileSystem);

            return Task.Run(() =>
            {
                var fullDstPath = destinationPath.FullPath;
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
                // be thrown (the UnauthorizedAccessException below). To not lose it with other TFMs,
                // only include the check in the failing .NET Core versions.
                EnsureNoConflictingFolderExists(fullDstPath.ToString());
#endif

                try
                {
                    File.Copy(_fullPath.ToString(), fullDstPath.ToString(), overwrite);
                    return FileSystem.GetFile(destinationPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    EnsureNoConflictingFolderExists(fullDstPath.ToString(), ex);
                    throw;
                }
            }, cancellationToken);
        }

        public override Task<StorageFile> MoveAsync(
            StoragePath destinationPath,
            NameCollisionOption options,
            CancellationToken cancellationToken = default
        )
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            destinationPath = destinationPath.ToPhysicalStoragePath(FileSystem);

            return Task.Run(() =>
            {
                EnsureExists(cancellationToken);

                var fullDestinationPath = destinationPath.FullPath;
                var overwrite = options.ToOverwriteBool();

                // System.IO doesn't throw when moving files to the same location.
                // Detecting this via paths will not always work, but it fulfills the spec most of the time.
                if (_fullPath == fullDestinationPath)
                {
                    throw new IOException(ExceptionStrings.StorageFile.CannotMoveToSameLocation());
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
            }, cancellationToken);
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
                    ExceptionStrings.StorageFile.NewNameContainsInvalidChar(FileSystem.PathInformation),
                    nameof(newName)
                );
            }

            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            var destinationPath = _fullParentPath.Join(newName);
            return MoveAsync(destinationPath, options, cancellationToken);
        }

        public override Task DeleteAsync(DeletionOption options, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(options))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(options), nameof(options));
            }

            return Task.Run(
                options switch 
                {
                    DeletionOption.Fail => FailImpl,
                    DeletionOption.IgnoreMissing => IgnoreMissingImpl,
                    _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(options))
                },
                cancellationToken
            );

            void FailImpl()
            {
                EnsureExists(cancellationToken);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    File.Delete(_fullPath.ToString());
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                }
            }

            void IgnoreMissingImpl()
            {
                try
                {
                    File.Delete(_fullPath.ToString());
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                }
            }
        }

        public override Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default)
        {
            if (!EnumInfo.IsDefined(fileAccess))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(fileAccess), nameof(fileAccess));
            }

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
            }, cancellationToken);
        }

        public override Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(async () =>
            {
                try
                {
                    return await FilePolyfills.ReadAllBytesMaybeAsync(_fullPath.ToString(), cancellationToken).ConfigureAwait(false);
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
            return Task.Run(async () =>
            {
                EnsureExists(cancellationToken);
                await FilePolyfills.WriteAllBytesMaybeAsync(_fullPath.ToString(), bytes, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        public override Task<string> ReadTextAsync(Encoding? encoding, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                try
                {
                    return encoding is null
                        ? await FilePolyfills.ReadAllTextMaybeAsync(_fullPath.ToString(), cancellationToken).ConfigureAwait(false)
                        : await FilePolyfills.ReadAllTextMaybeAsync(_fullPath.ToString(), encoding, cancellationToken).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EnsureNoConflictingFolderExists(_fullPath.ToString(), ex);
                    throw;
                }
            }, cancellationToken);
        }

        public override Task WriteTextAsync(
            string text,
            Encoding? encoding,
            CancellationToken cancellationToken = default
        )
        {
            _ = text ?? throw new ArgumentNullException(nameof(text));
            return Task.Run(async () =>
            {
                EnsureExists(cancellationToken);

                if (encoding is null)
                {
                    await FilePolyfills.WriteAllTextMaybeAsync(_fullPath.ToString(), text, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await FilePolyfills.WriteAllTextMaybeAsync(_fullPath.ToString(), text, encoding, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken);
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

        private static void EnsureNoConflictingFolderExists(string path, Exception? innerException = null)
        {
            if (Directory.Exists(path))
            {
                throw new IOException(ExceptionStrings.StorageFile.ConflictingFolderExistsAtFileLocation(), innerException);
            }
        }
    }
}
