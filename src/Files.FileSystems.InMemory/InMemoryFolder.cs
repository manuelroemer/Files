namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Files;
    using System.Threading;
    using Files.FileSystems.InMemory.Fs;
    using Files.FileSystems.InMemory.Resources;

    internal sealed class InMemoryFolder : Folder
    {

        private readonly InMemoryFileSystem _fileSystem;
        private readonly InMemoryPath _path;
        private readonly VirtualFileSystemStorage _storage;

        public override FileSystem FileSystem => _fileSystem;

        public override Path Path => _path;

        public InMemoryFolder(InMemoryFileSystem fileSystem, InMemoryPath path)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _storage = fileSystem.Storage;
        }

        public override Folder? GetParent()
        {
            return _path.Parent is null
                ? null
                : new InMemoryFolder(_fileSystem, (InMemoryPath)_path.Parent);
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_fileSystem.Lock)
            {
                var exists = VirtualFolder.Exists(_storage, _path);
                return Task.FromResult(exists);
            }
        }

        public override Task CreateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_fileSystem.Lock)
            {
                VirtualFolder.Create(_storage, _path, openExisting: true);
                return Task.CompletedTask;
            }
        }

        public override Task DeleteAsync(DeletionMode deletionOption, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_fileSystem.Lock)
            {
                VirtualFolder.Get(_storage, _path).Delete();
                return Task.CompletedTask;
            }
        }

        public override Task<Folder> CopyAsync(Path destinationPath, NameCollisionOption options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Folder> MoveAsync(Path destinationPath, NameCollisionOption options, CancellationToken cancellationToken = default)
        {
            _ = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            cancellationToken.ThrowIfCancellationRequested();

            lock (_fileSystem.Lock)
            {
                var virtualFolder = VirtualFolder.Get(_storage, _path);
                var overwrite = options switch
                {
                    NameCollisionOption.Fail => false,
                    NameCollisionOption.ReplaceExisting => true,
                    _ => throw new NotSupportedException()
                };
                
                virtualFolder.Move((InMemoryPath)destinationPath, overwrite);
                var result = new InMemoryFolder(_fileSystem, (InMemoryPath)destinationPath);
                return Task.FromResult<Folder>(result);
            }
        }

        public override Task<Folder> RenameAsync(string newName, NameCollisionOption options, CancellationToken cancellationToken = default)
        {
            _ = newName ?? throw new ArgumentNullException(nameof(newName));
            cancellationToken.ThrowIfCancellationRequested();

            lock (_fileSystem.Lock)
            {
                // We must ensure that the new name is not a path (e.g. foo/bar). The easiest way to do
                // that is to just create a path and check the segments.
                var namePath = _fileSystem.GetPath(newName);
                if (_fileSystem.GetPath(newName).Segments.Count != 1)
                {
                    throw new ArgumentException(ExceptionStrings.InMemoryFileSystemElement.NameIsPath(newName), nameof(newName));
                }

                // As with most real FS implementations, we can just reuse Move for doing the rename.
                // This requires us to pass the destination path. If we don't have a parent, we are at a
                // root directory. These cannot be renamed, but Move takes care of that. That's why we
                // can just pass in the newName as a path.
                var destination = GetParent()?.Path?.JoinWith(newName) ?? namePath;
                return MoveAsync(destination, options, cancellationToken);
            }
        }

        public override Task<File> CreateFileAsync(string name, CreationCollisionOption options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Folder> CreateFolderAsync(string name, CreationCollisionOption options, CancellationToken cancellationToken = default)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            cancellationToken.ThrowIfCancellationRequested();

            lock (_fileSystem.Lock)
            {
                var newFolderPath = (InMemoryPath)Path.JoinWith(name);
                VirtualFolder.Create(_storage, newFolderPath, )
            }
        }

        public override Task<IEnumerable<FileSystemElement>> GetAllChildrenAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<File>> GetAllFilesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<Folder>> GetAllFoldersAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<File> GetFileAsync(string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Folder> GetFolderAsync(string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<File> OpenFileAsync(string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<Folder> OpenFolderAsync(string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }

}
