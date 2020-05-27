namespace Files.FileSystems.InMemory.Internal
{
    using System.IO;
    using Files;

    internal sealed class FileNode : ElementNode
    {
        public new FolderNode Parent  => base.Parent ?? throw new IOException("Parent is null for a file.");

        public FileContent Content { get; private set; } = new FileContent();

        private FileNode(FsDataStorage storage, StoragePath path, FolderNode parent)
            : base(storage, path, parent) { }

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
            newNode.Content = Content.Copy();
        }

        public override void EnsureNotLocked()
        {
            Content.ReadWriteTracker.EnsureCanReadWrite();
        }
    }
}
