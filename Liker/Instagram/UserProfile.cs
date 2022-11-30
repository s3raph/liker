namespace Liker.Instagram
{
    public class UserProfile
    {
        public string UserName   { get; set; }
        public string UserId     { get; set; }
        public int FollowerCount { get; set; }
        public bool FollowedByViewer { get; internal set; }
        public bool FollowsViewer { get; internal set; }
        public bool HasBlockedViewer { get; internal set; }
    }
}