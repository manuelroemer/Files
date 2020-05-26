using System;
using System.Diagnostics;
using System.IO;

namespace Files.FileSystems.InMemory.Internal
{
    [DebuggerDisplay("{Path}")]
    internal abstract class ElementNode
    {
        public FsDataStorage Storage { get; }

        public StoragePath Path { get; private set; }

        public FolderNode? Parent { get; private set; }

        public FileAttributes Attributes { get; set; }

        public DateTimeOffset CreatedOn { get; protected set; }

        public DateTimeOffset? ModifiedOn { get; protected set; }

        protected ElementNode(FsDataStorage storage, StoragePath path, FolderNode? parent)
        {
            Storage = storage;
            Parent = parent;
            Path = path.FullPath;
            ModifiedOn = CreatedOn = DateTimeOffset.Now;
        }

        public void Copy(StoragePath destinationPath, bool replaceExisting)
        {
            if (Storage.IsSameElement(Path, destinationPath))
            {
                throw new IOException("The element cannot be moved to the same location.");
            }

            if (replaceExisting)
            {
                // TODO: Consider replacing the following type-based if with proper inheritance.
                if (this is FileNode)
                {
                    Storage.TryGetFileNodeAndThrowOnConflictingFolder(destinationPath)?.Delete();
                }
                else if (this is FolderNode)
                {
                    Storage.TryGetFolderNodeAndThrowOnConflictingFile(destinationPath)?.Delete();
                }
            }

            Storage.EnsureNoConflictingNodeExists(destinationPath);

            CopyImpl(destinationPath);
        }

        protected abstract void CopyImpl(StoragePath destinationPath);

        public virtual void Move(StoragePath destinationPath, bool replaceExisting)
        {
            if (Storage.IsSameElement(Path, destinationPath))
            {
                throw new IOException("The element cannot be moved to the same location.");
            }

            if (Parent is null)
            {
                throw new IOException("A rooted element cannot be moved to another location.");
            }

            var newParentNode = Storage.GetParentNode(destinationPath);
            if (newParentNode is null)
            {
                throw new IOException("A non rooted element cannot be moved into a root location.");
            }

            FolderNode? currentParent = newParentNode;
            do
            {
                if (ReferenceEquals(currentParent, this))
                {
                    throw new IOException("A parent folder cannot be moved into one of its child folders.");
                }
            } while ((currentParent = currentParent.Parent) is object);

            // Moving can be done by re-registering the node associations. There is no need
            // to create/clone new nodes.
            // Before updating the registrations, we must preemptively ensure that no conflicting
            // node exists at the move location. Otherwise `Storage.RegisterNode(this)` might throw
            // when the associations have already been mutated.
            // Care must also be taken with the properties to be updated. Anything that depends
            // on the path must be updated.
            if (replaceExisting)
            {
                // TODO: Consider replacing the following type-based if with proper inheritance.
                if (this is FileNode)
                {
                    Storage.TryGetFileNodeAndThrowOnConflictingFolder(destinationPath)?.Delete();
                }
                else if (this is FolderNode)
                {
                    Storage.TryGetFolderNodeAndThrowOnConflictingFile(destinationPath)?.Delete();
                }
            }

            Storage.EnsureNoConflictingNodeExists(destinationPath);

            Parent.UnregisterChildNode(this);
            Storage.UnregisterNode(this);

            Parent = newParentNode;
            Path = destinationPath.FullPath;

            Storage.RegisterNode(this);
            newParentNode.RegisterChildNode(this);

            OnModified();
        }

        public virtual void Delete()
        {
            Parent?.UnregisterChildNode(this);
            Storage.UnregisterNode(this);
        }

        protected void OnModified()
        {
            ModifiedOn = DateTimeOffset.Now;
        }
    }
}
