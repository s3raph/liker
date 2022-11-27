namespace Liker.Instagram
{
    public class AccountFollower
    {
        public string UserID      { get; init; }

        public string Username    { get; init; }

        public bool Following     { get; init; }

        public bool IsPrivate     { get; init; }

        public bool IsRestricted  { get; init; }

        public int? FollowerCount { get; set; } = null;
    }
}
