namespace Files.FileSystems.Physical
{
    using System;
    using System.IO;
    using Files;
    using Files.Shared;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using static System.Environment;
    using static System.Environment.SpecialFolder;

    /// <summary>
    ///     A <see cref="FileSystem"/> implementation which uses the <see cref="System.IO"/> API
    ///     for interacting with the local physical file system.
    ///     See remarks for details.
    /// </summary>
    /// <remarks>
    ///     The <see cref="PhysicalFileSystem"/> uses the <see cref="System.IO"/> API
    ///     for interacting with the local file system.
    ///     It is therefore compatible with any other file system implementation using the path format
    ///     of the underlying operating system.
    ///     
    ///     While it is possible without errors to create multiple instances of the
    ///     <see cref="PhysicalFileSystem"/>, you should ideally create and reuse a single
    ///     instance of this class.
    /// </remarks>
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
        public override StoragePath GetPath(KnownFolder knownFolder)
        {
            if (!EnumInfo.IsDefined(knownFolder))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(knownFolder), nameof(knownFolder));
            }

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
                _ => throw new NotSupportedException(ExceptionStrings.FileSystem.KnownFolderNotSupported(knownFolder)),
            };
            return GetPath(path);

            static string GetSpecialFolder(SpecialFolder specialFolder) =>
                GetFolderPath(specialFolder, SpecialFolderOption.DoNotVerify);
        }
    }
}
