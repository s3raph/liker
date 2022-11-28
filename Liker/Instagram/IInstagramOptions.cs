namespace Liker.Instagram
{
    public interface IInstagramOptions
    {
        string CSRFToken                   { get; set; }
        string SessionID                   { get; set; }

        /// <summary>
        /// Max calls allowed to be made to /api/v1/users/web_profile_info/
        ///
        /// <seealso cref="InstagramService"/> will throw a <seealso cref="InstagramRESTException"/> once
        /// this threshold has been exceeded.
        /// </summary>
        int MaxAllowedUserProfileInfoCalls { get; }
    }
}