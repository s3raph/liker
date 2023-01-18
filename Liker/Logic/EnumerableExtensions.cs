namespace Liker.Logic
{
    internal static class EnumerableExtensions
    {
        private static Random Randy = new Random();

        /// <summary>
        /// Returns a quantity of random elements from the source.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements of source.
        /// </typeparam>
        /// <param name="values"></param>
        /// <param name="quantityToTake">
        /// The quantity of elements to take. If the quantity is greater than the source, the source is returned.
        /// </param>
        /// <returns>
        /// A sequence that contains the specified number of random elements from the source sequence.
        /// </returns>
        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> values, int quantityToTake)
        {
            var valuesCollection = values.ToArray();

            // If the quantity is greater than the source, the source is returned.
            if (valuesCollection.Length <= quantityToTake)
            {
                return values;
            }

            var randomlySelectedIndexes = new SortedSet<int>();

            // Keep selecting random indexes until we have the quantity we need.
            while (randomlySelectedIndexes.Count < quantityToTake)
            {
                randomlySelectedIndexes.Add(Randy.Next(valuesCollection.Length));
            }

            // Return the elements at the randomly selected indexes.
            return randomlySelectedIndexes.Select(i => valuesCollection[i]);
        }
    }
}
