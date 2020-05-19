namespace Files.FileSystems.InMemory.Internal
{
    using System.Collections.Generic;
    using System.IO;

    internal sealed class FsDataStorage
    {
        private readonly Dictionary<StoragePath, ElementNode> _allNodes;
        private readonly List<FolderNode> _rootFolderNodes;

        public FsDataStorage(IEqualityComparer<StoragePath> pathComparer)
        {
            _allNodes = new Dictionary<StoragePath, ElementNode>(pathComparer);
            _rootFolderNodes = new List<FolderNode>();
        }

        public void RegisterNode(ElementNode node)
        {
            EnsureNoConflictingNodeExists(node.Path);
            _allNodes.Add(node.Path.FullPath, node);

            if (node is FolderNode folderNode && folderNode.Parent is null)
            {
                _rootFolderNodes.Add(folderNode);
            }
        }

        public void UnregisterNode(ElementNode node)
        {
            _allNodes.Remove(node.Path.FullPath);

            if (node is FolderNode folderNode)
            {
                _rootFolderNodes.Remove(folderNode);
            }
        }

        public void EnsureNoConflictingNodeExists(StoragePath path)
        {
            var existingNode = TryGetElementNode(path);

            if (existingNode is FileNode)
            {
                throw new IOException($"A conflicting file exists at {path}.");
            }

            if (existingNode is FolderNode)
            {
                throw new IOException($"A conflicting folder exists at {path}.");
            }
        }

        public bool HasFileNode(StoragePath path) =>
            TryGetFileNode(path) is object;

        public bool HasFolderNode(StoragePath path) =>
            TryGetFolderNode(path) is object;

        public bool HasElementNode(StoragePath path) =>
            TryGetElementNode(path) is object;

        public FolderNode GetRequiredParentNode(StoragePath path) =>
            GetParentNode(path) ?? throw new IOException($"The path {path} is required to have a parent, but has none.");

        public FolderNode? GetParentNode(StoragePath path) =>
            path.FullPath.Parent is null ? null : GetFolderNode(path.FullPath.Parent);

        public FileNode GetFileNode(StoragePath path) =>
            TryGetFileNode(path) ?? throw new FileNotFoundException($"No file exists at {path}.", path.FullPath.ToString());

        public FolderNode GetFolderNode(StoragePath path) =>
            TryGetFolderNode(path) ?? throw new DirectoryNotFoundException($"No folder exists at {path}.");

        public FileNode? TryGetFileNode(StoragePath path) =>
            TryGetElementNode(path) as FileNode;

        public FolderNode? TryGetFolderNode(StoragePath path) =>
            TryGetElementNode(path) as FolderNode;

        public ElementNode? TryGetElementNode(StoragePath path) =>
            _allNodes.TryGetValue(path.FullPath, out var node) ? node : null;
    }
}
