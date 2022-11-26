using System.Collections.ObjectModel;

namespace Liker.Instagram
{
    public class Page<T> : ReadOnlyCollection<T>
    {
        public bool IsThereAnotherPage { get; internal set; }
        public PageOptions? NextPageOptions { get; internal set; }

        public Page(IList<T> list) : base(list) { }
    }
}