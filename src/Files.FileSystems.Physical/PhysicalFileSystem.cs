namespace Files.FileSystems.Physical
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Files;
    using static System.Environment;
    using static System.Environment.SpecialFolder;

    public sealed class PhysicalFileSystem : FileSystem
    {

        private static readonly PathInformation PhysicalPathInformation = new PathInformation(
            Path.GetInvalidPathChars(),
            Path.GetInvalidFileNameChars(),
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
            extensionSeparatorChar: '.',
            currentDirectorySegment: ".",
            parentDirectorySegment: ".."
        );

        /// <summary>
        ///     Gets a default instance of the <see cref="PhysicalFileSystem"/> class.
        /// </summary>
        public static PhysicalFileSystem Default { get; } = new PhysicalFileSystem();

        /// <inheritdoc/>
        public override PathInformation PathInformation => PhysicalPathInformation;

        /// <inheritdoc/>
        public override StorageFile GetFile(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalStorageFile(this, (PhysicalStoragePath)path);
        }

        /// <inheritdoc/>
        public override StorageFolder GetFolder(StoragePath path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalFolder(this, (PhysicalStoragePath)path);
        }

        /// <inheritdoc/>
        public override StoragePath GetPath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalStoragePath(this, path);
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
                KnownFolder.UsersProfile => GetSpecialFolder(UserProfile),
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
