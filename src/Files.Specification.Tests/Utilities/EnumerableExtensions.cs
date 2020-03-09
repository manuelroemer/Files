namespace Files.Specification.Tests.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Defines static utility methods for deconstructing enumerables in tuples.
    ///     This is introduced to allow convenient setup of file and folder structures with
    ///     immediate access to the objects:
    ///     
    ///     <code>
    ///     // SetupFolderAsync returns an IEnumerable.
    ///     var (src, dst) = await folder.SetupFolderAsync(
    ///         basePath => basePath / "src",
    ///         basePath => basePath / "src"
    ///     );
    ///     </code>
    /// </summary>
    public static class EnumerableExtensions
    {

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T t1, out T t2)
        {
            t1 = enumerable.ElementAt(0);
            t2 = enumerable.ElementAt(1);
        }

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T t1, out T t2, out T t3)
        {
            t1 = enumerable.ElementAt(0);
            t2 = enumerable.ElementAt(1);
            t3 = enumerable.ElementAt(2);
        }

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T t1, out T t2, out T t3, out T t4)
        {
            t1 = enumerable.ElementAt(0);
            t2 = enumerable.ElementAt(1);
            t3 = enumerable.ElementAt(2);
            t4 = enumerable.ElementAt(3);
        }

    }

}
