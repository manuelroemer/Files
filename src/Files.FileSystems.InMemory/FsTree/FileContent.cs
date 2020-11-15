namespace Files.FileSystems.InMemory.FsTree
{
    using System;
    using System.IO;
    using Files.Shared;

    internal sealed class FileContent
    {
        private readonly FileNode _ownerFileNode;
        private byte[] _content = Array.Empty<byte>();

        public ulong Size => (ulong)_content.LongLength;

        public FileContentReadWriteTracker ReadWriteTracker { get; } = new FileContentReadWriteTracker();

        public FileContent(FileNode ownerFileNode)
        {
            _ownerFileNode = ownerFileNode;
        }

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

            void Stream_Disposed(object? sender, EventArgs e)
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
                        _ownerFileNode.SetModifiedToNow();
                        break;
                    case FileAccess.ReadWrite:
                        ReadWriteTracker.CloseReaderWriter();
                        _content = stream.ToArray();
                        _ownerFileNode.SetModifiedToNow();
                        break;
                }
            }
        }

        public FileContent Copy(FileNode newOwnerFileNode)
        {
            ReadWriteTracker.EnsureCanRead();

            return new FileContent(newOwnerFileNode)
            {
                _content = (byte[])_content.Clone(),
            };
        }
    }
}
