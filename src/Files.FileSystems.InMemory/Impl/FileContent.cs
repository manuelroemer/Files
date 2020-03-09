namespace Files.FileSystems.InMemory.Impl
{
    using System;
    using System.IO;

    /// <summary>
    ///     Provides access to the content of an in-memory file.
    ///     Such a file's content consists of nothing else but a byte array to which users of the API
    ///     can gain read and write access.
    ///     
    ///     This class simulates real file systems in the sense that it restricts read and write
    ///     access under certain conditions if a file is already in use.
    ///     At the moment, users can:
    ///     - Read content if there are no other writers.
    ///     - Write content if there are no other readers and writers.
    /// </summary>
    internal sealed class FileContent
    {

        private readonly object _lock = new object();
        private readonly ReadWriteTracker _readWriteTracker = new ReadWriteTracker();
        private byte[] _data = Array.Empty<byte>();

        public Stream Open(FileAccess fileAccess)
        {
            lock (_lock)
            {
                if (fileAccess == FileAccess.Read)
                {
                    _readWriteTracker.EnsureReadAccess();
                    _readWriteTracker.IncrementReaders();
                    return CreateStream(fileAccess);
                }
                else if (fileAccess == FileAccess.Write)
                {
                    _readWriteTracker.EnsureWriteAccess();
                    _readWriteTracker.IncrementWriters();
                    return CreateStream(fileAccess);
                }
                else
                {
                    _readWriteTracker.EnsureReadAccess();
                    _readWriteTracker.EnsureWriteAccess();
                    _readWriteTracker.IncrementReaders();
                    _readWriteTracker.IncrementWriters();
                    return CreateStream(fileAccess);
                }
            }
        }

        private FileContentStream CreateStream(FileAccess fileAccess)
        {
            var stream = new FileContentStream(_data, fileAccess);
            stream.Disposed += Stream_Disposed;
            return stream;

            void Stream_Disposed(object sender, EventArgs e)
            {
                // When the stream is closed, we have to reset the reader/writer count to
                // ensure that new calls to Open won't throw.
                // In addition, if the stream had write access, the data might have changed.
                // In such cases, replace the internal array with the new one to propagate the
                // changes into this class.
                lock (_lock)
                {
                    stream.Disposed -= Stream_Disposed;

                    if (fileAccess == FileAccess.Read)
                    {
                        _readWriteTracker.DecrementReaders();
                        // A read-only stream cannot change _data, therfore there's no need to do a copy.
                    }
                    else if (fileAccess == FileAccess.Write)
                    {
                        _readWriteTracker.DecrementWriters();
                        _data = stream.ToArray();
                    }
                    else
                    {
                        _readWriteTracker.DecrementReaders();
                        _readWriteTracker.DecrementWriters();
                        _data = stream.ToArray();
                    }
                }
            }
        }
    }

}
