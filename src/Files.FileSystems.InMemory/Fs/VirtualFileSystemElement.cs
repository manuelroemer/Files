namespace Files.FileSystems.InMemory.Fs
{
    using System;
    using System.IO;
    using Files.FileSystems.InMemory.Resources;

    internal abstract class VirtualFileSystemElement
    {

        public VirtualFileSystemStorage Storage { get; }

        public VirtualFolder? Parent { get; private set; }

        public InMemoryPath Path { get; private set; }

        private protected VirtualFileSystemElement(VirtualFileSystemStorage storage, InMemoryPath path)
        {
            Storage = storage;
            Path = path;
            Parent = Path.Parent is null ? null : VirtualFolder.Get(storage, (InMemoryPath)Path.Parent);

            // New elements must be tracked in the storage (for global access) and via the
            // parent-child relations (so that the FS tree can easily be traversed).
            // Setup the storage first, because this may fail. In such a case, the P/C relation is untouched.
            if (!Storage.TryAdd(this))
            {
                throw new IOException(ExceptionStrings.IOExceptions.ElementAlreadyExists(path));
            }

            SetParentChildRelation(newParent: Parent);
        }

        public virtual void Delete()
        {
            if (!Storage.TryRemove(this))
            {
                throw CreateNotFoundException();
            }

            SetParentChildRelation(newParent: null);
        }

        public virtual void Move(InMemoryPath destination, bool overwrite)
        {
            // Renames to make the method easier to read.
            var source = Path;
            var newParentPath = (InMemoryPath?)destination.Normalize().Parent;

            // Cannot move root directories.
            if (newParentPath is null)
            {
                throw new IOException(ExceptionStrings.IOExceptions.CannotMoveRootDirectory((Files.Path)destination));
            }

            // Cannot move parent directories into child directories.
            if (source.IsParentPathOf(destination))
            {
                throw new IOException(ExceptionStrings.IOExceptions.CannotMoveParentIntoChild(source, destination));
            }

            // The destination must not exist if we are not overwriting (Conflict).
            if (!overwrite && Storage.ContainsElement(destination))
            {
                throw new IOException(ExceptionStrings.IOExceptions.ElementAlreadyExists(destination));
            }

            // The new parent directory must exist.
            if (!Storage.ContainsElement(newParentPath))
            {
                throw new IOException(ExceptionStrings.IOExceptions.DestinationParentNotFound(newParentPath));
            }

            // Nothing to do if the locations are equal anyway.
            if (source.LocationEquals(destination))
            {
                return;
            }

            // If we overwrite, remove any existing old element first.
            if (overwrite)
            {
                var elementAtDestination = Storage.TryGetElement(destination);

                if (elementAtDestination is object)
                {
                    elementAtDestination.Delete();
                }
            }

            // Do the actual moving.
            // Hacky, but important: Update Path while Storage does not hold a reference to this
            // element, because Storage uses the current Path value in TryAdd.
            Storage.TryRemove(this);
            Path = destination;
            Storage.TryAdd(this);

            var newParent = Storage.TryGetFolder(newParentPath)!; // We have already verified that this exists.
            SetParentChildRelation(newParent);
        }
        
        protected void SetParentChildRelation(VirtualFolder? newParent)
        {
            // Clear any previous relation.
            if (Parent is object)
            {
                Parent.Children.Remove(this);
                Parent = null;
            }

            if (newParent is object)
            {
                Parent = newParent;
                Parent.Children.Add(this);
            }
        }

        protected abstract IOException CreateNotFoundException();

    }

}
