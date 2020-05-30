namespace Files.FileSystems.InMemory.FsTree
{
    using System.Collections.Generic;
    using System.IO;
    using Files.Shared;

    /// <summary>
    ///     The InMemoryFileSystem is, like a real file system, based on a file/folder tree structure.
    ///     This class is the entry point to these trees, i.e. it stores and manages the root nodes.
    ///     In addition, it provides methods for quickly finding and interacting with specific nodes.
    ///     
    ///     This class (aswell as the associated node classes) is not doing any thread synchronization.
    ///     Instead, the user must ensure that the FS tree is only accessed synchronously.
    /// </summary>
    internal sealed class FsDataStorage
    {
        private readonly Dictionary<StoragePath, ElementNode> _allNodes;
        private readonly List<FolderNode> _rootFolderNodes;

        /// <summary>
        ///     Gets a dictionary which tracks all available file/folder nodes.
        ///     These nodes are uniquely identified via their path. For efficient access, that path
        ///     is used as the dictionary's key. This enables not having to search the entire
        ///     tree when looking for a node at a specific path.
        /// </summary>
        public IReadOnlyDictionary<StoragePath, ElementNode> AllNodes => _allNodes;
        
        /// <summary>
        ///     Gets a list of all tracked root folder nodes, i.e. those folder nodes which don't
        ///     have a parent node.
        /// </summary>
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
            GetParentNode(path) ?? throw new IOException(ExceptionStrings.StoragePath.PathHasNoParent());

        public FolderNode? GetParentNode(StoragePath path) =>
            path.FullPath.Parent is null ? null : GetFolderNode(path.FullPath.Parent);

        public FileNode GetFileNode(StoragePath path)
        {
            var fileNode = TryGetFileNodeAndThrowOnConflictingFolder(path);

            if (fileNode is null)
            {
                if (path.FullPath.Parent is object && HasFolderNode(path.FullPath.Parent))
                {
                    throw new FileNotFoundException(ExceptionStrings.StorageFile.NotFound(path));
                }
                else
                {
                    throw new DirectoryNotFoundException(ExceptionStrings.StorageFile.ParentNotFound(path));
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
                    throw new DirectoryNotFoundException(ExceptionStrings.StorageFolder.NotFound(path));
                }
                else
                {
                    throw new DirectoryNotFoundException(ExceptionStrings.StorageFolder.ParentNotFound(path));
                }
            }

            return folderNode;
        }

        public FileNode? TryGetFileNodeAndThrowOnConflictingFolder(StoragePath path)
        {
            EnsureNoConflictingFolderNodeExistsAt(path);
            return TryGetFileNode(path);
        }

        public FolderNode? TryGetFolderNodeAndThrowOnConflictingFile(StoragePath path)
        {
            EnsureNoConflictingFileNodeExistsAt(path);
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
            EnsureNoConflictingFileNodeExistsAt(path);
            EnsureNoConflictingFolderNodeExistsAt(path);
        }

        private void EnsureNoConflictingFileNodeExistsAt(StoragePath path)
        {
            if (HasFileNode(path))
            {
                throw new IOException(ExceptionStrings.StorageFolder.ConflictingFileExistsAtFolderLocation());
            }
        }

        private void EnsureNoConflictingFolderNodeExistsAt(StoragePath path)
        {
            if (HasFolderNode(path))
            {
                throw new IOException(ExceptionStrings.StorageFile.ConflictingFolderExistsAtFileLocation());
            }
        }
    }
}
