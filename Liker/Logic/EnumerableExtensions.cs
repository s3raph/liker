namespace Liker.Logic
{
    internal static class EnumerableExtensions
    {
        private static Random Randy = new Random();

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> values, int quantityToTake)
        {
            var valuesCollection = values.ToArray();

            if (valuesCollection.Length <= quantityToTake)
            {
                return values;
            }

            var randomlySelectedIndexes = new SortedSet<int>();

            while (randomlySelectedIndexes.Count < quantityToTake)
            {
                randomlySelectedIndexes.Add(Randy.Next(valuesCollection.Length));
            }

            return randomlySelectedIndexes.Select(i => valuesCollection[i]);
        }
    }
}
