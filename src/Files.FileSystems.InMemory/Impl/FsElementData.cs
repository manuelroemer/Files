namespace Files.FileSystems.InMemory.Impl
{
    using System;

    internal abstract class FsElementData
    {

        public FolderData? Parent { get; set; }

        public InMemoryPath Path { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? CreationTime { get; set; }

        public FsElementData(InMemoryPath path, FolderData? parent = null)
        {
            Path = path;
            Parent = parent;
            Name = (path.FileName ?? path.DirectoryName)!;
            CreationTime = DateTimeOffset.Now;
        }

    }

}
