namespace Files.FileSystems.InMemory.Internal
{
    using System.Diagnostics;
    using System.IO;

    internal sealed class FileNode : ElementNode
    {
        public new FolderNode Parent  => base.Parent ?? throw new IOException("Parent is null for a file.");

        public FileContent Content { get; } = new FileContent();

        private FileNode(FsDataStorage storage, StoragePath path, FolderNode parent)
            : base(storage, path, parent) { }

        public static FileNode Create(FsDataStorage storage, StoragePath path)
        {
            var parentNode = storage.GetRequiredParentNode(path);
            var node = new FileNode(storage, path, parentNode);
            storage.RegisterNode(node);
            parentNode.RegisterChildNode(node);
            return node;
        }
    }
}
