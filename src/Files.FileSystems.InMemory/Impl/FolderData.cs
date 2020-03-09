namespace Files.FileSystems.InMemory.Impl
{
    using System.Collections.Generic;
    using Files;

    internal sealed class FolderData : FsElementData
    {

        public IList<FileData> Files { get; } = new List<FileData>();

        public IList<FolderData> Folders { get; } = new List<FolderData>();

        public FolderData(Path path)
            : base(path) { }

    }

}
