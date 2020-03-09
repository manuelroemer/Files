namespace Files.FileSystems.InMemory.Fs
{
    using System;
    using System.IO;

    internal static class IOExceptionHelper
    {

        public static IOException CreateNotFoundException(VirtualFileSystemElement element, string? message) =>
            element switch
            {
                VirtualFile _ => new FileNotFoundException(message),
                VirtualFolder _ => new DirectoryNotFoundException(message),
                _ => throw new NotImplementedException()
            };

    }

}
