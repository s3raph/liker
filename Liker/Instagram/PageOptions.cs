namespace Liker.Instagram
{
    public class PageOptions
    {
        public int PageSize { get; init; } = 12;
        public string? MaxID { get; init; }
        public bool HasMaxID => !string.IsNullOrEmpty(MaxID);

        public string AsQueryString()
        {
            if (HasMaxID)
            {
                return $"count={PageSize}&max_id={MaxID}";
            }
            else
            {
                return $"count={PageSize}";
            }
        }
    }
}