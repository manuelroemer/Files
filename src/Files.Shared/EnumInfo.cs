namespace Files.Shared
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     A rather hacky class which provides enum information to the guard functions.
    ///     This class only works with the "default" enums, i.e. enums which are based on
    ///     <see cref="int"/> and, if flagged, have the <see cref="FlagsAttribute"/> and follow
    ///     the typical flags number/value order.
    ///     
    ///     At the time of writing this, this is good enough for all cases in this library.
    ///     If a need for a more sophisticated solution arises, we should most likely just
    ///     use the Enums.NET library instead of this.
    /// </summary>
    internal static class EnumInfo
    {
        internal static bool IsDefined<T>(T value) where T : struct, Enum, IConvertible
        {
            var intValue = value.ToInt32(CultureInfo.InvariantCulture);
            return EnumCache<T>.IsFlagged
                ? (EnumCache<T>.CompositeFlaggedValue & intValue) == intValue
                : Enum.IsDefined(typeof(T), value);
        }

        private static class EnumCache<T> where T : struct, Enum, IConvertible
        {
            public static bool IsFlagged { get; }
            public static int CompositeFlaggedValue { get; }

            static EnumCache()
            {
                Debug.Assert(
                    typeof(T).GetEnumUnderlyingType() == typeof(int),
                    "Only use enum functionalities with enumerations based on Int32."
                );

                IsFlagged = typeof(T).GetCustomAttribute<FlagsAttribute>() is not null;

                if (IsFlagged)
                {
                    CompositeFlaggedValue = ((T[])Enum.GetValues(typeof(T)))
                        .Select(x => x.ToInt32(CultureInfo.InvariantCulture))
                        .Aggregate((l, r) => l | r);
                }
            }
        }
    }
}
