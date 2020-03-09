namespace Files.FileSystems.InMemory.Fs
{
    using System.Collections.Generic;
    using Files.FileSystems.InMemory;

    internal sealed class InMemoryPathEqualityComparer : EqualityComparer<InMemoryPath?>
    {

        public static new InMemoryPathEqualityComparer Default { get; } = new InMemoryPathEqualityComparer();

        public override bool Equals(InMemoryPath? first, InMemoryPath? second)
        {
            if (first is object && second is object)
            {
                return first.LocationEquals(second);
            }
            else
            {
                return first is null && second is null;
            }
        }

        public override int GetHashCode(InMemoryPath? path)
        {
            return path?.GetHashCode() ?? 0;
        }

    }

}
