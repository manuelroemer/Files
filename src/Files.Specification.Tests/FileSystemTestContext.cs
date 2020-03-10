namespace Files.Specification.Tests
{
    using System.Threading.Tasks;

    public abstract class FileSystemTestContext
    {

        public abstract FileSystem FileSystem { get; }

        public abstract Task<StorageFolder> GetTestFolderAsync();

    }

}
