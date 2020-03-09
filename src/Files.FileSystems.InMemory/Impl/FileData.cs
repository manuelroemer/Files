namespace Files.FileSystems.InMemory.Impl
{
    internal sealed class FileData : FsElementData
    {

        public string NameWithoutExtension { get; set; }

        public string? Extension { get; set; }

        public FileData(InMemoryPath path, FolderData? parent = null)
            : base(path, parent)
        {
            NameWithoutExtension = path.FileNameWithoutExtension!;
            Extension = path.Extension;
        }

    }

}
