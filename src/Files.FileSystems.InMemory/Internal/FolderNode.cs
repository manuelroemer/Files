namespace Files.FileSystems.InMemory.Internal
{
    using System.Collections.Generic;
    using Files;

    internal sealed class FolderNode : ElementNode
    {
        // Keeps track of the children of this folder. This list is unordered.
        private readonly List<ElementNode> _mutableChildren = new List<ElementNode>();

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

        public override void Move(StoragePath destinationPath, bool replaceExisting)
        {
            base.Move(destinationPath, replaceExisting);
            MoveChildrenToNewLocation();
        }

        private void MoveChildrenToNewLocation()
        {
            // Cannot use foreach because Move(..) mutates the list and thus invalidates the enumerator.
            // Because of this, we must also iterate from behind, because the order in the list changes here.
            for (var i = _mutableChildren.Count - 1; i >= 0; i--)
            {
                var child = _mutableChildren[i];
                var newLocation = Path.FullPath.Join(child.Path.FullPath.Name);
                child.Move(newLocation, replaceExisting: false);
            }
        }

        protected override void CopyImpl(StoragePath destinationPath)
        {
            var newNode = Create(Storage, destinationPath);
            newNode.Attributes = Attributes;
            newNode.CreatedOn = CreatedOn;
            newNode.ModifiedOn = ModifiedOn;

            foreach (var child in _mutableChildren)
            {
                var childDestinationPath = destinationPath.FullPath.Join(child.Path.FullPath.Name);
                child.Copy(childDestinationPath, replaceExisting: false);
            }
        }

        public override void Delete()
        {
            // Because deleting a child can fail, it is essential that the children are deleted before
            // this folder node.
            // This ensures that a user can retry deleting if something goes wrong.

            while (_mutableChildren.Count > 0)
            {
                // Delete() automatically removes the element from the list, hence no foreach/additional logic.
                _mutableChildren[_mutableChildren.Count - 1].Delete();
            }

            base.Delete();
        }
    }
}
