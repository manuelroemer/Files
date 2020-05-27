namespace Files.FileSystems.InMemory.Internal
{
    using System;
    using System.IO;
    using Files.Shared;

    internal sealed class FileContent
    {
        private byte[] _content = Array.Empty<byte>();

        public ulong Size => (ulong)_content.LongLength;

        public FileContentReadWriteTracker ReadWriteTracker { get; } = new FileContentReadWriteTracker();

        public FileContentStream Open(FileAccess fileAccess, bool replaceExistingContent)
        {
            switch (fileAccess)
            {
                case FileAccess.Read:
                    ReadWriteTracker.AddReader();
                    break;
                case FileAccess.Write:
                    ReadWriteTracker.AddWriter();
                    break;
                case FileAccess.ReadWrite:
                    ReadWriteTracker.AddReaderWriter();
                    break;
                default:
                    throw new NotSupportedException(ExceptionStrings.Enum.UnsupportedValue(fileAccess));
            }

            var content = replaceExistingContent ? Array.Empty<byte>() : _content;
            var stream = new FileContentStream(content, fileAccess);
            stream.Disposed += Stream_Disposed;
            return stream;

            void Stream_Disposed(object sender, EventArgs e)
            {
                stream.Disposed -= Stream_Disposed;

                switch (fileAccess)
                {
                    case FileAccess.Read:
                        ReadWriteTracker.CloseReader();
                        break;
                    case FileAccess.Write:
                        ReadWriteTracker.CloseWriter();
                        _content = stream.ToArray();
                        break;
                    case FileAccess.ReadWrite:
                        ReadWriteTracker.CloseReaderWriter();
                        _content = stream.ToArray();
                        break;
                }
            }
        }

        public FileContent Copy()
        {
            ReadWriteTracker.EnsureCanRead();

            return new FileContent()
            {
                _content = (byte[])_content.Clone(),
            };
        }
    }
}
