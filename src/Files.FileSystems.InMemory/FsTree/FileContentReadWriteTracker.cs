namespace Files.FileSystems.InMemory.FsTree
{
    using System.IO;
    using Files.Shared;

    /// <summary>
    ///     A very simple implementation of a reader/writer tracker for the <see cref="FileContent"/>
    ///     which ensures that concurrent reading/writing is not possible.
    ///     
    ///     This class is specifically introduced in favor of thread-based solutions like
    ///     ReaderWriterLockSlim because the InMemoryFileSystem is currently single-threaded anyway.
    ///     We must only ensure here that the reader/writer tracking is appropriately handled on 
    ///     the Dispose() call of the FileContentStreams (which might be done on another thread).
    ///     Apart from that, it's just simple tracking of whether the content is currently open or not.
    /// </summary>
    internal sealed class FileContentReadWriteTracker
    {
        // While this class has a public API surface which differentiates between readers/writers,
        // the internal implementation only allows a single reader/writer right now.
        // This is done to match the FileShare.None behavior in the PhysicalFileSystem.
        // To make changes easier in the future, methods like AddReader/AddWriter have already been
        // added now though, even though they internally do the same.
        // Future extensions might change the behavior based on a FileShare value.

        private readonly object _lock = new object();
        private bool _isLocked;

        public void AddReader()
        {
            lock (_lock)
            {
                EnsureCanReadInternal();
                _isLocked = true;
            }
        }

        public void AddWriter()
        {
            lock (_lock)
            {
                EnsureCanWriteInternal();
                _isLocked = true;
            }
        }

        public void AddReaderWriter()
        {
            lock (_lock)
            {
                EnsureCanReadWriteInternal();
                _isLocked = true;
            }
        }

        public void CloseReader()
        {
            lock (_lock)
            {
                _isLocked = false;
            }
        }

        public void CloseWriter()
        {
            lock (_lock)
            {
                _isLocked = false;
            }
        }

        public void CloseReaderWriter()
        {
            lock (_lock)
            {
                _isLocked = false;
            }
        }

        public void EnsureCanRead()
        {
            lock (_lock)
            {
                EnsureCanReadInternal();
            }
        }

        public void EnsureCanWrite()
        {
            lock (_lock)
            {
                EnsureCanWriteInternal();
            }
        }
        
        public void EnsureCanReadWrite()
        {
            lock (_lock)
            {
                EnsureCanReadWriteInternal();
            }
        }

        private void EnsureCanReadInternal()
        {
            if (_isLocked)
            {
                throw new IOException(ExceptionStrings.StorageFile.FileIsLocked());
            }
        }

        private void EnsureCanWriteInternal()
        {
            if (_isLocked)
            {
                throw new IOException(ExceptionStrings.StorageFile.FileIsLocked());
            }
        }

        private void EnsureCanReadWriteInternal()
        {
            EnsureCanReadInternal();
            EnsureCanWriteInternal();
        }
    }
}
