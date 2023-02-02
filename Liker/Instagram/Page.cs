using System.Collections.ObjectModel;

namespace Liker.Instagram
{
    /// <summary>
    /// Represents a page of items retrieved from Instagram
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T> : ReadOnlyCollection<T>
    {
        /// <summary>
        /// True if there are more pages available after this page.
        /// </summary>
        public bool IsThereAnotherPage => !string.IsNullOrEmpty(NextPageOptions?.MaxID);

        /// <summary>
        /// The options to use to get the next page.
        /// </summary>
        public PageOptions? NextPageOptions { get; init; }

        public Page(IList<T> list) : base(list) { }
    }
}