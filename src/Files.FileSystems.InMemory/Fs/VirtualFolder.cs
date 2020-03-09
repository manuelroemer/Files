namespace Files.FileSystems.InMemory.Fs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Files.FileSystems.InMemory.Resources;

    internal sealed class VirtualFolder : VirtualFileSystemElement
    {

        public IList<VirtualFileSystemElement> Children { get; }

        private VirtualFolder(VirtualFileSystemStorage storage, InMemoryPath path)
            : base(storage, path)
        {
            Children = new List<VirtualFileSystemElement>();
        }

        public static VirtualFolder Create(VirtualFileSystemStorage storage, InMemoryPath path, CreationCollisionOption options)
        {
            if (storage.ContainsFile(path))
            {
                throw new IOException(ExceptionStrings.IOExceptions.FileBlocksCreation(path));
            }

            // If a folder already exists, handle it according to the provided options.
            var existingFolder = storage.TryGetFolder(path);
            if (existingFolder is object)
            {
                if (options == CreationCollisionOption.Fail)
                {
                    throw new IOException(ExceptionStrings.IOExceptions.ElementAlreadyExists(path));
                }
                else if (options == CreationCollisionOption.ReplaceExisting)
                {
                    existingFolder.Delete();
                }
                else if (options == CreationCollisionOption.Open)
                {
                    return existingFolder;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            
            // At this point, no element exists.
            return new VirtualFolder(storage, path);
        }

        public static VirtualFolder Get(VirtualFileSystemStorage storage, InMemoryPath path)
        {
            return storage.TryGetElement(path) switch
            {
                VirtualFolder folder => folder,
                VirtualFile _ => throw new IOException(ExceptionStrings.IOExceptions.MismatchExpectedFileButGotFolder(path)),
                _ => throw new DirectoryNotFoundException(ExceptionStrings.IOExceptions.ElementNotFound(path))
            };
        }

        public static bool Exists(VirtualFileSystemStorage storage, InMemoryPath path)
        {
            return storage.TryGetFolder(path) is object;
        }

        public override void Delete()
        {
            foreach (var child in Children)
            {
                child.Delete();
            }

            base.Delete();
        }

        public override void Move(InMemoryPath destination, bool overwrite)
        {
            // Move this folder before moving the children so that the destination always exists.
            base.Move(destination, overwrite);

            foreach (var child in Children)
            {
                var childDestination = (InMemoryPath)destination.JoinWith(child.Path.Name);
                child.Move(childDestination, overwrite);
            }
        }

        protected override IOException CreateNotFoundException()
        {
            return new DirectoryNotFoundException(ExceptionStrings.IOExceptions.ElementNotFound(Path));
        }

    }

}
