namespace Liker.Instagram
{
    /// <summary>
    /// Options for working with <see cref="InstagramService"/>.
    /// </summary>
    public interface IInstagramOptions
    {
        /// <summary>
        /// Value for the x-csrftoken header and csrftoken cookie
        /// </summary>
        string CSRFToken                   { get; set; }

        /// <summary>
        /// Value for the sessionid cookie
        /// </summary>
        string SessionID                   { get; set; }

        /// <summary>
        /// Max calls allowed to be made to /api/v1/users/web_profile_info/
        ///
        /// <seealso cref="InstagramService"/> will throw a <seealso cref="InstagramRESTException"/> once
        /// this threshold has been exceeded.
        /// </summary>
        int MaxAllowedUserProfileInfoCalls { get; }

        /// <summary>
        /// Value for the x-ig-www-claim header
        /// </summary>
        string IGWWWClaim { get; }

        /// <summary>
        /// Value for the x-instagram-ajax header
        /// </summary>
        string IGAjax { get; }
    }
}