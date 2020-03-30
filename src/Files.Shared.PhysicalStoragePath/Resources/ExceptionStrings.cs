namespace Files.Shared.PhysicalStoragePath.Resources
{
    internal static class ExceptionStrings
    {
        internal static class PhysicalStoragePath
        {
            internal static string InvalidFormat() =>
                $"The specified path has an invalid format. See the inner exception for details.";

            internal static string TrimmingResultsInEmptyPath() =>
                $"Trimming the trailing directory separator results in an empty path string which " +
                $"is not supported by the {nameof(StoragePath)} class.";

            internal static string TrimmingResultsInInvalidPath() =>
                "Trimming the trailing directory separator results in an invalid path string. " +
                "See the inner exception for details.";
        }
    }
}
