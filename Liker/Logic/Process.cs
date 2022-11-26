using Liker.Instagram;

namespace Liker.Logic
{
    internal class Process
    {
        private readonly IInstagramService InstaService;

        private readonly PageOptions GetFollowersFirstPageOptions = new();

        public Process(IInstagramService instaService)
        {
            InstaService = instaService ?? throw new ArgumentNullException(nameof(instaService));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="accountsToProcess"></param>
        /// <param name="token"></param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public async Task Run(IEnumerable<string> accountsToProcess, CancellationToken token = default)
        {
            // foreach account
            foreach (var account in accountsToProcess)
            {
                // foreach follower page
                Page<AccountFollower> currentPage = null;

                do
                {
                    token.ThrowIfCancellationRequested();

                    currentPage = await InstaService.GetUserFollowersAsync(account, currentPage?.NextPageOptions ?? GetFollowersFirstPageOptions);

                    // foreach follower
                    foreach (var follower in currentPage)
                    {
                        token.ThrowIfCancellationRequested();

                        // insert into database
                        // if !private AND !blocked AND !seenBefore AND followers < 400
                        //   retrieve first NUM (default 9) follower posts (and thumbnails)
                        //   if can find posts with known hashtags
                        //     Like random selection of <5 posts
                        //   else
                        //     find any photos that don't contain a face
                        //     Like one random post
                    }
                }
                while (currentPage.IsThereAnotherPage);
            }
        }
    }
}
