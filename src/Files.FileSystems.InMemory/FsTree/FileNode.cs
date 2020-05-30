namespace Files.FileSystems.InMemory.FsTree
{
    using System.IO;
    using Files;

    /// <see cref="ElementNode"/>
    internal sealed class FileNode : ElementNode
    {
        public new FolderNode Parent  => base.Parent!;

        public FileContent Content { get; private set; }

        private FileNode(FsDataStorage storage, StoragePath path, FolderNode parent)
            : base(storage, path, parent)
        {
            Content = new FileContent(ownerFileNode: this);
            Attributes = FileAttributes.Normal;
        }

        public static FileNode Create(FsDataStorage storage, StoragePath path)
        {
            var parentNode = storage.GetParentNodeAndRequirePathToHaveParent(path);
            var node = new FileNode(storage, path, parentNode);
            storage.RegisterNode(node);
            parentNode.RegisterChildNode(node);
            return node;
        }

        protected override void CopyImpl(StoragePath destinationPath)
        {
            var newNode = Create(Storage, destinationPath);
            newNode.Attributes = Attributes;
            newNode.CreatedOn = CreatedOn;
            newNode.ModifiedOn = ModifiedOn;
            newNode.Content = Content.Copy(newOwnerFileNode: newNode);
        }

        public override void EnsureNotLocked()
        {
            Content.ReadWriteTracker.EnsureCanReadWrite();
        }

        protected override void DeleteSameNodeTypeAtPathIfExisting(StoragePath destinationPath)
        {
            Storage.TryGetFileNodeAndThrowOnConflictingFolder(destinationPath)?.Delete();
        }
    }
}
