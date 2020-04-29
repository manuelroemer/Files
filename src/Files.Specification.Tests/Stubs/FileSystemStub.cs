namespace Files.Specification.Tests.Stubs
{
    using System;
    using System.IO;
    using Files;

    /// <summary>
    ///     A <see cref="FileSystem"/> providing incredibly simplified, canned results.
    ///     This is mainly introduced for testing foreign file system support.
    /// </summary>
    public sealed class FileSystemStub : FileSystem
    {
        public override PathInformation PathInformation { get; } = new PathInformation(
            Path.GetInvalidPathChars(),
            Path.GetInvalidFileNameChars(),
            '/',
            '/',
            '.',
            ':',
            ".",
            "..",
            StringComparison.Ordinal
        );

        public override StorageFile GetFile(StoragePath path) =>
            throw new NotImplementedException();

        public override StorageFolder GetFolder(StoragePath path) =>
            throw new NotImplementedException();

        public override StoragePath GetPath(string path) =>
            new StoragePathStub(this, path);

        public override StoragePath GetPath(KnownFolder knownFolder) =>
            GetPath(knownFolder.ToString());
    }
}
