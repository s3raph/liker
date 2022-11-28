namespace Liker.Logic
{
    public interface IProcessOptions
    {
        int DelaySeed { get; set; }
        int RuntimeLimit { get; set; }
        IEnumerable<string> HashTagsToLike { get; }

    }
}