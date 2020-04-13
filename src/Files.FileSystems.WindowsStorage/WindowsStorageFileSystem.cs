namespace Files.FileSystems.WindowsStorage
{
    using System;
    using Files;
    using Files.Shared.PhysicalStoragePath;
    using Files.Shared.PhysicalStoragePath.Utilities;
    using Files.FileSystems.WindowsStorage.Utilities;
    using Files.Shared;
    using Windows.Storage;
    using static System.Environment;
    using StorageFile = StorageFile;
    using StorageFolder = StorageFolder;

    /// <summary>
    ///     A <see cref="FileSystem"/> implementation which uses the <see cref="Windows.Storage"/>
    ///     API of the Windows SDK for interacting with the local physical file system.
    ///     See remarks for details.
    /// </summary>
    /// <remarks>
    ///     The <see cref="WindowsStorageFileSystem"/> uses the <see cref="Windows.Storage"/> API
    ///     for interacting with the local file system.
    ///     Therefore, all restrictions which apply to a sandboxed application also apply to this
    ///     file system. This means that, by default, most operations on locations which are not
    ///     accessible out of the box will throw an <see cref="UnauthorizedAccessException"/>.
    ///     To access additional locations, you must be granted access by the application' user,
    ///     for example via the <see cref="Windows.Storage.Pickers.FileOpenPicker"/>.
    ///     
    ///     Due to these restrictions, this file system implementation returns application specific
    ///     paths for certain <see cref="KnownFolder"/> values, e.g. the application's own temporary
    ///     data folder.
    ///     
    ///     Apart from this, this file system implementation is compatible with any other file system
    ///     implementation using the Win32 path format. Apart from the <see cref="KnownFolder"/>
    ///     differences, it should be possible to switch between this implementation and similar ones
    ///     (for example the <c>PhysicalFileSystem</c>) without too much effort.
    ///     
    ///     While it is possible without errors to create multiple instances of the
    ///     <see cref="WindowsStorageFileSystem"/>, you should ideally create and reuse a single
    ///     instance of this class.
    /// </remarks>
    public sealed class WindowsStorageFileSystem : FileSystem
    {
        /// <inheritdoc/>
        public override PathInformation PathInformation => PhysicalPathHelper.PhysicalPathInformation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowsStorageFileSystem"/> class.
        /// </summary>
        public WindowsStorageFileSystem() { }

        /// <inheritdoc/>
        public override StorageFile GetFile(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new WindowsStorageStorageFile(this, path.ToPhysicalStoragePath(this));
        }

        /// <inheritdoc/>
        public override StorageFolder GetFolder(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new WindowsStorageStorageFolder(this, path.ToPhysicalStoragePath(this));
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
            if (!Enum.IsDefined(typeof(KnownFolder), knownFolder))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(knownFolder), nameof(knownFolder));
            }

            var path = knownFolder switch
            {
                KnownFolder.TemporaryData => ApplicationData.Current.TemporaryFolder.GetPathOrThrow(),
                KnownFolder.RoamingApplicationData => ApplicationData.Current.RoamingFolder.GetPathOrThrow(),
                KnownFolder.LocalApplicationData => ApplicationData.Current.LocalFolder.GetPathOrThrow(),
                KnownFolder.ProgramData => GetSpecialFolder(SpecialFolder.CommonApplicationData),
                KnownFolder.UserProfile => GetSpecialFolder(SpecialFolder.UserProfile),
                KnownFolder.Desktop => GetSpecialFolder(SpecialFolder.Desktop),
                KnownFolder.DocumentsLibrary => GetSpecialFolder(SpecialFolder.MyDocuments),
                KnownFolder.PicturesLibrary => GetSpecialFolder(SpecialFolder.MyPictures),
                KnownFolder.VideosLibrary => GetSpecialFolder(SpecialFolder.MyVideos),
                KnownFolder.MusicLibrary => GetSpecialFolder(SpecialFolder.MyMusic),
                _ => throw new NotSupportedException(ExceptionStrings.FileSystem.KnownFolderNotSupported(knownFolder)),
            };
            return GetPath(path);

            static string GetSpecialFolder(SpecialFolder specialFolder) =>
                GetFolderPath(specialFolder, SpecialFolderOption.DoNotVerify);
        }
    }
}
