namespace Files.FileSystems.Physical
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Files;
    using Files.FileSystems.Physical.Utilities;
    using Files.FileSystems.Shared.PhysicalStoragePath;
    using Files.FileSystems.Shared.PhysicalStoragePath.Utilities;
    using static System.Environment;
    using static System.Environment.SpecialFolder;

    /// <summary>
    ///     A <see cref="FileSystem"/> implementation which uses the <see cref="System.IO"/> API
    ///     for interacting with the local physical file system.
    /// </summary>
    public sealed class PhysicalFileSystem : FileSystem
    {
        /// <summary>
        ///     Gets a default instance of the <see cref="PhysicalFileSystem"/> class.
        /// </summary>
        public static PhysicalFileSystem Default { get; } = new PhysicalFileSystem();

        /// <inheritdoc/>
        public override PathInformation PathInformation => PhysicalPathHelper.PhysicalPathInformation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PhysicalFileSystem"/> class.
        /// </summary>
        public PhysicalFileSystem() { }

        /// <inheritdoc/>
        public override StorageFile GetFile(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalStorageFile(this, path.ToPhysicalStoragePath(this));
        }

        /// <inheritdoc/>
        public override StorageFolder GetFolder(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalStorageFolder(this, path.ToPhysicalStoragePath(this));
        }

        /// <inheritdoc/>
        public override StoragePath GetPath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalStoragePath(path, this);
        }

        /// <inheritdoc/>
        public override bool TryGetPath(KnownFolder knownFolder, [NotNullWhen(true)] out StoragePath? result)
        {
            var path = knownFolder switch
            {
                KnownFolder.TemporaryData => Path.GetTempPath(),
                KnownFolder.RoamingApplicationData => GetSpecialFolder(ApplicationData),
                KnownFolder.LocalApplicationData => GetSpecialFolder(LocalApplicationData),
                KnownFolder.ProgramData => GetSpecialFolder(CommonApplicationData),
                KnownFolder.UserProfile => GetSpecialFolder(UserProfile),
                KnownFolder.Desktop => GetSpecialFolder(Desktop),
                KnownFolder.DocumentsLibrary => GetSpecialFolder(MyDocuments),
                KnownFolder.PicturesLibrary => GetSpecialFolder(MyPictures),
                KnownFolder.VideosLibrary => GetSpecialFolder(MyVideos),
                KnownFolder.MusicLibrary => GetSpecialFolder(MyMusic),
                _ => null,
            };

            return TryGetPath(path, out result);

            static string GetSpecialFolder(SpecialFolder specialFolder) =>
                GetFolderPath(specialFolder, SpecialFolderOption.DoNotVerify);
        }
    }
}
