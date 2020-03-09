namespace Files.FileSystems.Physical
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Files;
    using System.Diagnostics.CodeAnalysis;
    using IOPath = System.IO.Path;
    using System.Xml.XPath;
    using Files.FileSystems.Physical.Resources;
    using static System.Environment.SpecialFolder;
    using static System.Environment;
    using System.Runtime.CompilerServices;

    public sealed class PhysicalFileSystem : FileSystem
    {

        private static readonly PathInformation PhysicalPathInformation = new PathInformation(
            IOPath.GetInvalidPathChars(),
            IOPath.GetInvalidFileNameChars(),
            IOPath.DirectorySeparatorChar,
            IOPath.AltDirectorySeparatorChar,
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
        public override File GetFile(Path path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalFile(this, (PhysicalPath)path);
        }

        /// <inheritdoc/>
        public override Folder GetFolder(Path path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalFolder(this, (PhysicalPath)path);
        }

        /// <inheritdoc/>
        public override Path GetPath(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new PhysicalPath(this, path);
        }

        /// <inheritdoc/>
        public override bool TryGetPath(KnownFolder knownFolder, [NotNullWhen(true)] out Path? result)
        {
            var path = knownFolder switch
            {
                KnownFolder.TemporaryData => IOPath.GetTempPath(),
                KnownFolder.RoamingApplicationData => GetSpecialFolder(ApplicationData),
                KnownFolder.LocalApplicationData => GetSpecialFolder(LocalApplicationData),
                KnownFolder.ProgramData => GetSpecialFolder(CommonApplicationData),
                KnownFolder.UsersProfile => GetSpecialFolder(UserProfile),
                KnownFolder.Desktop => GetSpecialFolder(Desktop),
                KnownFolder.DocumentsLibrary => GetSpecialFolder(MyDocuments),
                KnownFolder.PicturesLibrary => GetSpecialFolder(MyPictures),
                KnownFolder.VideosLibrary => GetSpecialFolder(MyVideos),
                KnownFolder.MusicLibrary => GetSpecialFolder(MyMusic),
                _ => throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(knownFolder)),
            };

            return TryGetPath(path, out result);

            static string GetSpecialFolder(SpecialFolder specialFolder) =>
                GetFolderPath(specialFolder, SpecialFolderOption.DoNotVerify);
        }

    }

}
