namespace Files.FileSystems.InMemory.Impl
{
    using System.IO;
    using Files.FileSystems.InMemory.Resources;

    /// <summary>
    ///     A utility class which tracks how many readers and writers are currently accessing an
    ///     element (a file's content in the context of this library).
    /// </summary>
    internal sealed class ReadWriteTracker
    {

        public int Readers { get; private set; }

        public int Writers { get; private set; }

        public void IncrementReaders()
        {
            Readers++;
        }

        public void DecrementReaders()
        {
            Readers--;
        }

        public void IncrementWriters()
        {
            Writers++;
        }

        public void DecrementWriters()
        {
            Writers--;
        }

        public bool HasReadAccess()
        {
            return Writers == 0;
        }

        public bool HasWriteAccess()
        {
            return Readers == 0 && Writers == 0;
        }

        public void EnsureReadAccess()
        {
            if (!HasReadAccess())
            {
                throw new IOException(ExceptionStrings.ReadWriteTracker.ElementInUse());
            }
        }

        public void EnsureWriteAccess()
        {
            if (!HasWriteAccess())
            {
                throw new IOException(ExceptionStrings.ReadWriteTracker.ElementInUse());
            }
        }

    }

}
