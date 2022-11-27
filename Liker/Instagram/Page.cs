using System.Collections.ObjectModel;

namespace Liker.Instagram
{
    public class Page<T> : ReadOnlyCollection<T>
    {
        public bool IsThereAnotherPage { get; init; }
        public PageOptions? NextPageOptions { get; init; }

        public Page(IList<T> list) : base(list) { }
    }
}