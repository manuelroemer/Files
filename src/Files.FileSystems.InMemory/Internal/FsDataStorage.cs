namespace Files.FileSystems.InMemory.Internal
{
    using System.Collections.Generic;
    using System.IO;

    internal sealed class FsDataStorage
    {
        private readonly Dictionary<StoragePath, ElementNode> _allNodes;
        private readonly List<FolderNode> _rootFolderNodes;

        public IReadOnlyDictionary<StoragePath, ElementNode> AllNodes => _allNodes;
        public IReadOnlyList<FolderNode> RootFolderNodes => _rootFolderNodes;

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

        public bool IsSameElement(StoragePath path1, StoragePath path2) =>
            ReferenceEquals(TryGetElementNode(path1), TryGetElementNode(path2));

        public bool HasFileNode(StoragePath path) =>
            TryGetFileNode(path) is object;

        public bool HasFolderNode(StoragePath path) =>
            TryGetFolderNode(path) is object;

        public bool HasElementNode(StoragePath path) =>
            TryGetElementNode(path) is object;

        public FolderNode GetParentNodeAndRequirePathToHaveParent(StoragePath path) =>
            GetParentNode(path) ?? throw new IOException($"The path {path} is required to have a parent, but has none.");

        public FolderNode? GetParentNode(StoragePath path) =>
            path.FullPath.Parent is null ? null : GetFolderNode(path.FullPath.Parent);

        public FileNode GetFileNode(StoragePath path)
        {
            var fileNode = TryGetFileNodeAndThrowOnConflictingFolder(path);

            if (fileNode is null)
            {
                if (path.FullPath.Parent is object && HasFolderNode(path.FullPath.Parent))
                {
                    throw new FileNotFoundException($"The file at {path} does not exist.");
                }
                else
                {
                    throw new DirectoryNotFoundException($"One or more parent folders of the file at {path} don't exist.");
                }
            }

            return fileNode;
        }

        public FolderNode GetFolderNode(StoragePath path)
        {
            var folderNode = TryGetFolderNodeAndThrowOnConflictingFile(path);

            if (folderNode is null)
            {
                if (path.FullPath.Parent is object && HasFolderNode(path.FullPath.Parent))
                {
                    throw new DirectoryNotFoundException($"The folder at {path} does not exist.");
                }
                else
                {
                    throw new DirectoryNotFoundException($"One or more parent folders of the folder at {path} don't exist.");
                }
            }

            return folderNode;
        }

        public FileNode? TryGetFileNodeAndThrowOnConflictingFolder(StoragePath path)
        {
            EnsureNoConflictingFolderNodeExists(path);
            return TryGetFileNode(path);
        }

        public FolderNode? TryGetFolderNodeAndThrowOnConflictingFile(StoragePath path)
        {
            EnsureNoConflictingFileNodeExists(path);
            return TryGetFolderNode(path);
        }

        public FileNode? TryGetFileNode(StoragePath path) =>
            TryGetElementNode(path) as FileNode;

        public FolderNode? TryGetFolderNode(StoragePath path) =>
            TryGetElementNode(path) as FolderNode;

        public ElementNode? TryGetElementNode(StoragePath path) =>
            _allNodes.TryGetValue(path.FullPath, out var node) ? node : null;

        public void EnsureNoConflictingNodeExists(StoragePath path)
        {
            EnsureNoConflictingFileNodeExists(path);
            EnsureNoConflictingFolderNodeExists(path);
        }

        private void EnsureNoConflictingFileNodeExists(StoragePath path)
        {
            if (HasFileNode(path))
            {
                throw new IOException($"A conflicting file exists at {path}.");
            }
        }

        private void EnsureNoConflictingFolderNodeExists(StoragePath path)
        {
            if (HasFolderNode(path))
            {
                throw new IOException($"A conflicting folder exists at {path}.");
            }
        }
    }
}
