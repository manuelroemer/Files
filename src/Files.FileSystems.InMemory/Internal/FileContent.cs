namespace Files.FileSystems.InMemory.Internal
{
    using System;
    using System.IO;

    internal sealed class FileContent
    {
        private byte[] _content = Array.Empty<byte>();

        public ulong Size => (ulong)_content.LongLength;

        public FileContentStream Open(FileAccess fileAccess, bool replaceExistingContent)
        {
            var content = replaceExistingContent ? Array.Empty<byte>() : _content;
            var stream = new FileContentStream(content, fileAccess);
            stream.Disposed += Stream_Disposed;
            return stream;

            void Stream_Disposed(object sender, EventArgs e)
            {
                stream.Disposed -= Stream_Disposed;

                if (stream.CanWrite)
                {
                    _content = stream.ToArray();
                }
            }
        }
    }
}
