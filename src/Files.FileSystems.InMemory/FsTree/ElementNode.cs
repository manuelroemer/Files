using System;
using System.Diagnostics;
using System.IO;

namespace Files.FileSystems.InMemory.FsTree
{
    /// <summary>
    ///     This class is the base class for the <see cref="FileNode"/> and <see cref="FolderNode"/>
    ///     and as such encapsulates the shared properties/logic which is used for maintaining
    ///     the in-memory file system tree.
    ///     
    ///     These node classes only provide operations which modify either the node's content or
    ///     the FS tree itself. They do not implement special behavior which is defined by the 
    ///     Files specification. This is the role of the <see cref="InMemoryStorageFile"/> and
    ///     <see cref="InMemoryStorageFolder"/>. These classes implement the specification based
    ///     on the provided methods in these node classes.
    ///     Of course, the nodes are designed with the specifications in mind, i.e. they throw
    ///     the expected exceptions when applicable.
    ///     
    ///     The node classes are not doing any thread synchronization.
    ///     Instead, the user must ensure that the FS tree is only accessed synchronously.
    /// </summary>
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
            EnsureNotLocked();

            if (Storage.IsSameElement(Path, destinationPath))
            {
                throw new IOException("The element cannot be moved to the same location.");
            }

            if (replaceExisting)
            {
                DeleteSameNodeTypeAtPathIfExisting(destinationPath);
            }

            Storage.EnsureNoConflictingNodeExists(destinationPath);

            CopyImpl(destinationPath);
        }

        protected abstract void CopyImpl(StoragePath destinationPath);

        public virtual void Move(StoragePath destinationPath, bool replaceExisting)
        {
            EnsureNotLocked();

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
                DeleteSameNodeTypeAtPathIfExisting(destinationPath);
            }

            Storage.EnsureNoConflictingNodeExists(destinationPath);

            Parent.UnregisterChildNode(this);
            Storage.UnregisterNode(this);

            Parent = newParentNode;
            Path = destinationPath.FullPath;

            Storage.RegisterNode(this);
            newParentNode.RegisterChildNode(this);

            SetModifiedToNow();
        }

        protected abstract void DeleteSameNodeTypeAtPathIfExisting(StoragePath destinationPath);

        public virtual void Delete()
        {
            EnsureNotLocked();
            Parent?.UnregisterChildNode(this);
            Storage.UnregisterNode(this);
        }

        public abstract void EnsureNotLocked();

        public void SetModifiedToNow()
        {
            ModifiedOn = DateTimeOffset.Now;
        }
    }
}
