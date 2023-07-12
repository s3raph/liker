namespace Liker.Logic
{
    internal class Utility
    {
        private static Random Randy = new Random();

        /// <summary>
        /// Delays by a random interval that will be 50% higher or lower than the milliseconds provided before calling the
        /// provided function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="millisecondsSeed"></param>
        /// <param name="toDebounce"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task DelayByRandom(int millisecondsSeed, CancellationToken token = default) => Task.Delay((millisecondsSeed / 2) + Randy.Next(millisecondsSeed), token);
    }
}
