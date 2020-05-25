namespace Files.FileSystems.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Files.Shared;

    public sealed class DefaultKnownFolderProvider : IKnownFolderProvider
    {
        public static DefaultKnownFolderProvider Default { get; } = new DefaultKnownFolderProvider();

        public StoragePath GetPath(InMemoryFileSystem fileSystem, KnownFolder knownFolder)
        {
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            if (!EnumInfo.IsDefined(knownFolder))
            {
                throw new ArgumentException(ExceptionStrings.Enum.UndefinedValue(knownFolder), nameof(knownFolder));
            }

            return fileSystem.GetPath(knownFolder.ToString()).FullPath;
        }
    }
}
