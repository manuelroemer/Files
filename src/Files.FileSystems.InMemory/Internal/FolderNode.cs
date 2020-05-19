namespace Files.FileSystems.InMemory.Internal
{
    using System.Collections.Generic;
    using System.IO;

    internal sealed class FolderNode : ElementNode
    {
        private readonly HashSet<ElementNode> _mutableChildren = new HashSet<ElementNode>();

        public IReadOnlyCollection<ElementNode> Children => _mutableChildren;

        private FolderNode(FsDataStorage storage, StoragePath path, FolderNode? parent)
            : base(storage, path, parent) { }

        public static FolderNode Create(FsDataStorage storage, StoragePath path)
        {
            var parentNode = storage.GetParentNode(path);
            var node = new FolderNode(storage, path, parentNode);
            storage.RegisterNode(node);
            parentNode?.RegisterChildNode(node);
            return node;
        }

        public void RegisterChildNode(ElementNode node)
        {
            _mutableChildren.Add(node);
        }

        public void UnregisterChildNode(ElementNode node)
        {
            _mutableChildren.Remove(node);
        }
    }
}
