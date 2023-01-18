namespace Liker.Instagram
{
    /// <summary>
    /// Service for working with Instagram.
    /// </summary>
    public interface IInstagramService
    {
        /// <summary>
        /// Get the list of the user's followers.
        /// </summary>
        /// <param name="userHandle"></param>
        /// <param name="pageOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Page<AccountFollower>> GetUserFollowersAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve the given user's feed of posts limited by the provided PageOptions.
        /// </summary>
        /// <param name="userHandle"></param>
        /// <param name="pageOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Page<Post>> GetUserFeedAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Like the given post.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task LikeAsync(string postId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve the given user's profile.
        /// </summary>
        /// <param name="userName">The user's handle.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<UserProfile> GetUserProfile(string userName, CancellationToken cancellationToken);
    }
}