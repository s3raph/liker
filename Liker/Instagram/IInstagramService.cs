namespace Liker.Instagram
{
    public interface IInstagramService
    {
        Task<Page<AccountFollower>> GetUserFollowersAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default);
        Task<Page<Post>> GetUserFeedAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default);
        Task LikeAsync(string postId, CancellationToken cancellationToken = default);

        Task<UserProfile> GetUserProfile(string userName, CancellationToken cancellationToken);
    }
}