namespace Files.FileSystems.InMemory.Impl
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A custom stream implementation wrapping a memory stream to operate on an in-memory byte array.
    ///     This implementation is specialized for reading/writing the content of an in-memory file.
    ///     It conditionally provides read and write access based on a <see cref="FileAccess"/> modifier
    ///     and ensures that an exception is thrown when reading/writing is forbidden.
    ///     This is in contrast to the <see cref="MemoryStream"/> which only throws in certain methods.
    ///     
    ///     An additional feature is the <see cref="Disposed"/> event which is used to listen for when
    ///     a file has "finished writing".
    /// 
    ///     Note: In theory, it's possible to directly inherit from <see cref="MemoryStream"/> and just
    ///     override the Read/Write methods. This would allow users to cast to <see cref="MemoryStream"/> though.
    ///     To prevent this, a <see cref="MemoryStream"/> is simply wrapped.
    /// </summary>
    internal sealed class FileContentStream : Stream
    {

        public event EventHandler? Disposed;

        private readonly FileAccess _fileAccess;
        private readonly MemoryStream _underlyingStream;
        private bool _isDisposed;

        public override bool CanRead =>
            (_fileAccess == FileAccess.Read || _fileAccess == FileAccess.ReadWrite) && _underlyingStream.CanRead;

        public override bool CanWrite =>
            (_fileAccess == FileAccess.Write || _fileAccess == FileAccess.ReadWrite) && _underlyingStream.CanWrite;

        public override bool CanSeek => _underlyingStream.CanSeek;

        public override bool CanTimeout => _underlyingStream.CanTimeout;

        public override int ReadTimeout
        {
            get => _underlyingStream.ReadTimeout;
            set => _underlyingStream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _underlyingStream.WriteTimeout;
            set => _underlyingStream.WriteTimeout = value;
        }

        public override long Length => _underlyingStream.Length;

        public override long Position
        {
            get => _underlyingStream.Position;
            set => _underlyingStream.Position = value;
        }

        public FileContentStream(byte[] content, FileAccess fileAccess)
        {
            _fileAccess = fileAccess;
            _underlyingStream = new MemoryStream(capacity: content.Length);
            _underlyingStream.Write(content, 0, content.Length);
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            _underlyingStream.Seek(offset, origin);

        public override void SetLength(long value) =>
            _underlyingStream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureCanRead();
            return _underlyingStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return _underlyingStream.ReadByte();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            EnsureCanRead();
            return _underlyingStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            EnsureCanRead();
            return _underlyingStream.EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureCanRead();
            return _underlyingStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCanWrite();
            _underlyingStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            EnsureCanWrite();
            _underlyingStream.WriteByte(value);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            EnsureCanWrite();
            return _underlyingStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            EnsureCanWrite();
            _underlyingStream.EndWrite(asyncResult);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureCanWrite();
            return _underlyingStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureCanRead();
            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Flush()
        {
            _underlyingStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _underlyingStream.FlushAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_isDisposed)
            {
                _isDisposed = true;
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        public byte[] ToArray()
        {
            return _underlyingStream.ToArray();
        }

        private void EnsureCanRead()
        {
            if (!CanRead)
            {
                throw new NotSupportedException();
            }
        }

        private void EnsureCanWrite()
        {
            if (!CanWrite)
            {
                throw new NotSupportedException();
            }
        }

    }

}
