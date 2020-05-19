using System;
using System.IO;

namespace Files.FileSystems.InMemory.Internal
{
    internal abstract class ElementNode
    {
        public FsDataStorage Storage { get; }

        public StoragePath Path { get; private set; }

        public FolderNode? Parent { get; private set; }

        public FileAttributes Attributes { get; set; }

        public DateTimeOffset CreatedOn { get; }

        public DateTimeOffset? ModifiedOn { get; private set; }

        protected ElementNode(FsDataStorage storage, StoragePath path, FolderNode? parent)
        {
            Storage = storage;
            Parent = parent;
            Path = path.FullPath;
            ModifiedOn = CreatedOn = DateTimeOffset.Now;
        }

        public void Move(StoragePath destinationPath)
        {
            if (ReferenceEquals(Storage.TryGetFolderNode(destinationPath), this))
            {
                throw new IOException("The element cannot be moved to the same location.");
            }

            if (Parent is null)
            {
                throw new IOException("A rooted element cannot be moved to another location.");
            }

            var destinationParentPath = destinationPath.FullPath.Parent;
            if (destinationParentPath is null)
            {
                throw new IOException("A non rooted element cannot be moved into a root location.");
            }

            var newParentNode = Storage.GetFolderNode(destinationParentPath);

            // Moving can be done by re-registering the node associations. There is no need
            // to create/clone new nodes.
            // Before updating the registrations, we must preemptively ensure that no conflicting
            // node exists at the move location. Otherwise `Storage.RegisterNode(this)` might throw
            // when the associations have already been mutated.
            // Care must also be taken with the properties to be updated. Anything that depends
            // on the path must be updated.
            Storage.EnsureNoConflictingNodeExists(destinationPath);

            Parent.UnregisterChildNode(this);
            Storage.UnregisterNode(this);

            Parent = newParentNode;
            Path = destinationPath.FullPath;

            Storage.RegisterNode(this);
            newParentNode.RegisterChildNode(this);

            OnModified();
        }

        public void Delete()
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
