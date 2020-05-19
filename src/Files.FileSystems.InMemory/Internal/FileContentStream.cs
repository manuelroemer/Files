namespace Files.FileSystems.InMemory.Internal
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A wrapper around a <see cref="MemoryStream"/> which is tailored towards reading/writing
    ///     content from/to an in-memory file.
    ///     As such, this stream has inbuilt support for FileAccess management and close-notifications.
    ///     
    ///     Most members simply forward the internal calls to the underlying <see cref="MemoryStream"/>
    ///     instance. Here, read/write validation might be added on top.
    /// </summary>
    internal sealed class FileContentStream : Stream
    {
        public event EventHandler? Disposed;

        private readonly FileAccess _fileAccess;
        private readonly MemoryStream _ms;

        public override bool CanRead => _fileAccess == FileAccess.Read || _fileAccess == FileAccess.ReadWrite;

        public override bool CanWrite => _fileAccess == FileAccess.Write || _fileAccess == FileAccess.ReadWrite;
        
        public override bool CanSeek => _ms.CanSeek;
        
        public override long Length => _ms.Length;

        public override bool CanTimeout => _ms.CanTimeout;

        public override int ReadTimeout
        {
            get => _ms.ReadTimeout;
            set => _ms.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _ms.WriteTimeout;
            set => _ms.WriteTimeout = value;
        }

        public override long Position
        {
            get => _ms.Position;
            set => _ms.Position = value;
        }

        public FileContentStream(byte[] initialBuffer, FileAccess fileAccess)
        {
            _ms = new MemoryStream();
            _ms.Write(initialBuffer, 0, initialBuffer.Length);
            _ms.Position = 0;
        }

        public byte[] ToArray() =>
            _ms.ToArray();

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            EnsureCanRead();
            return _ms.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult) =>
            _ms.EndRead(asyncResult);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureCanRead();
            return _ms.ReadAsync(buffer, offset, count, cancellationToken);
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureCanRead();
            return _ms.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            EnsureCanRead();
            return _ms.ReadByte();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            EnsureCanWrite();
            return _ms.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult) =>
            _ms.EndWrite(asyncResult);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureCanWrite();
            return _ms.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCanWrite();
            _ms.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            EnsureCanWrite();
            _ms.WriteByte(value);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureCanRead();
            return _ms.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void SetLength(long value)
        {
            EnsureCanWrite();
            _ms.SetLength(value);
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            _ms.Seek(offset, origin);

        public override void Flush() =>
            _ms.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _ms.FlushAsync(cancellationToken);

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
                _ms.Dispose();
            }
            finally
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

#if NETSTANDARD2_1
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureCanRead();
            _ms.CopyTo(destination, bufferSize);
        }

        public override int Read(Span<byte> buffer)
        {
            EnsureCanRead();
            return _ms.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureCanRead();
            return _ms.ReadAsync(buffer, cancellationToken);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureCanWrite();
            _ms.Write(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureCanWrite();
            return _ms.WriteAsync(buffer, cancellationToken);
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await base.DisposeAsync().ConfigureAwait(false);
                await _ms.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
#endif

        private void EnsureCanRead()
        {
            if (!CanRead)
            {
                throw new NotSupportedException("The stream does not support reading.");
            }
        }

        private void EnsureCanWrite()
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }
        }
    }
}
