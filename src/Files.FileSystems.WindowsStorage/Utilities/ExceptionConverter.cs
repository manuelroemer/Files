namespace Files.FileSystems.WindowsStorage.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    /// <summary>
    ///     The Windows Storage API is incredibly inconsistent with the exceptions being thrown
    ///     (from a .NET perspective), i.e. it frequently happens that <see cref="Exception"/>
    ///     is thrown instead of <see cref="IOException"/> or that <see cref="COMException"/> is
    ///     thrown.
    ///     
    ///     For fulfilling the specification, we obviously have to handle such cases.
    ///     Sometimes this can be done by checking the HResult of an exception and reacting
    ///     according to the table here:
    ///     https://docs.microsoft.com/en-us/windows/uwp/files/best-practices-for-writing-to-files
    ///     
    ///     This class is responsible for that, i.e. it's the "make-exceptions-sane" converter.
    /// </summary>
    internal static class ExceptionConverter
    {
        // Possible HResults. See this link for details: https://docs.microsoft.com/en-us/windows/uwp/files/best-practices-for-writing-to-files#common-error-codes-for-write-methods-of-the-fileio-and-pathio-classes
        private const int ErrorElementAlreadyExists = unchecked((int)0x800700B7);
        private const int ErrorAccessDenied = unchecked((int)0x80070005);
        private const int ErrorSharingViolation = unchecked((int)0x80070020);
        private const int ErrorUnableToRemoveReplaced = unchecked((int)0x80070497);
        private const int ErrorDiskFull = unchecked((int)0x80070070);
        private const int ErrorOutOfMemory = unchecked((int)0x8007000E);
        private const int ErrorFail = unchecked((int)0x80004005);

        internal static async Task<T> WithConvertedException<T>(this Task<T> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw Convert(ex);
            }
        }

        internal static async Task WithConvertedException(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw Convert(ex);
            }
        }

        internal static Exception Convert(Exception ex) => ex.HResult switch
        {
            ErrorElementAlreadyExists => new IOException(ex.Message, ex),
            ErrorAccessDenied => new UnauthorizedAccessException(ex.Message, ex),
            ErrorOutOfMemory => new IOException(ex.Message, ex),
            ErrorDiskFull => new IOException(ex.Message, ex),
            ErrorSharingViolation => new IOException(ex.Message, ex),
            ErrorUnableToRemoveReplaced => new IOException(ex.Message, ex),
            ErrorFail => new IOException(ex.Message, ex),
            _ => ex
        };
    }
}
