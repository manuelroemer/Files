namespace Files.Specification.Tests.Stubs
{
    using Files;

    /// <summary>
    ///     A <see cref="StoragePath"/> providing incredibly simplified, canned results.
    ///     This is mainly introduced for testing foreign file system support.
    /// </summary>
    public sealed class StoragePathStub : StoragePath
    {
        // This class is mainly introduced for having a foreign file system member.
        // The properties/methods (apart from ToString()) should ideally never be used and do not
        // have to make any sense, as long as they return a valid object.

        public override FileSystem FileSystem { get; }
        public override PathKind Kind => PathKind.Absolute;
        public override StoragePath? Root => this;
        public override StoragePath? Parent => null;
        public override StoragePath FullPath => this;
        public override string Name => ToString();
        public override string NameWithoutExtension => ToString();
        public override string? Extension => "";

        public StoragePathStub(FileSystem fileSystem, string path)
            : base(path)
        {
            FileSystem = fileSystem;
        }
    }
}
