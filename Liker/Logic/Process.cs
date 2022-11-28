using Liker.Instagram;
using Liker.Persistence;
using System.Text.RegularExpressions;

namespace Liker.Logic
{
    public class Process
    {
        private readonly IInstagramService InstaService;
        private readonly IDatabase Database;
        private readonly IProcessOptions Options;
        private readonly Regex HashTagRegex;

        public Process(IInstagramService instaService, IDatabase database, IProcessOptions options)
        {
            InstaService = instaService ?? throw new ArgumentNullException(nameof(instaService));
            Database     = database     ?? throw new ArgumentNullException(nameof(database));
            Options      = options      ?? throw new ArgumentNullException(nameof(options));

            HashTagRegex = new Regex($@"\s#({string.Join('|', options.HashTagsToLike.Select(h => h.Substring(1)).ToArray())})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
            Console.WriteLine($"Starting run for accounts {string.Join(", ", accountsToProcess.Select(a => $"@{a}"))}");

            int accountsVisited = 0;
            int accountsLiked   = 0;
            int postsLiked      = 0;

            try
            {
                // foreach account
                foreach (var account in accountsToProcess)
                {
                    // Pick up our place from where we were up to last time
                    var storedAccountInfo = await Database.GetAccountAsync(account);

                    Page<AccountFollower> currentPage = null;
                    PageOptions GetFollowersFirstPageOptions;

                    if (storedAccountInfo != default)
                    {
                        GetFollowersFirstPageOptions = new PageOptions
                        {
                            MaxID = storedAccountInfo.NextMaxId
                        };
                    }
                    else
                    {
                        GetFollowersFirstPageOptions = new();
                    }

                    // foreach follower page
                    do
                    {
                        token.ThrowIfCancellationRequested();

                        currentPage = await InstaService.GetUserFollowersAsync(account, currentPage?.NextPageOptions ?? GetFollowersFirstPageOptions);

                        // filter out followers if they exist in database
                        var databaseFollowers = await Database.GetFollowersDictionaryAsync(currentPage.Select(f => f.UserID).ToArray());
                        var filteredPage = currentPage.Where(f => !databaseFollowers.ContainsKey(f.UserID));

                        accountsVisited += filteredPage.Count();

                        // foreach follower
                        foreach (var follower in filteredPage)
                        {
                            token.ThrowIfCancellationRequested();

                            if (follower.IsRestricted || follower.IsPrivate)
                            {
                                // insert into database
                                await Database.InsertFollowerAsync(follower);
                            }
                            else
                            {
                                await DelayByRandom(Options.DelaySeed);

                                var userProfile = await InstaService.GetUserProfile(follower.Username, token);

                                // insert into database
                                follower.FollowerCount = userProfile.FollowerCount;
                                await Database.InsertFollowerAsync(follower);

                                // if !private AND !blocked AND !seenBefore AND followers < 400
                                if (userProfile.FollowerCount < 400)
                                {
                                    // retrieve first NUM (default 12) follower posts (and thumbnails)
                                    var posts = await InstaService.GetUserFeedAsync(follower.Username, new PageOptions(), token);

                                    var postsNotAlreadyLiked = posts.Where(p => !p.HasLiked);

                                    // if can find posts with known hashtags
                                    var postsWithTags = posts.Where(p => DoesTextContainAnyHashTags(p.Caption?.Text));

                                    if (postsWithTags.Any())
                                    {
                                        accountsLiked++;

                                        // Like random selection of <5 posts
                                        var randomPosts = postsWithTags.TakeRandom(5).ToList();

                                        foreach (var post in randomPosts)
                                        {
                                            await DelayByRandom(Options.DelaySeed);
                                            await InstaService.LikeAsync(post.Pk, token);
                                            postsLiked++;
                                        }

                                        Console.WriteLine($"Liked {randomPosts.Count} of @{follower.Username}'s posts");
                                    }
                                    else
                                    {
                                        // find any photos that don't contain a face
                                        // Like one random post

                                        // todo: Not implemented yet
                                    }
                                }
                            }
                        }

                        if (currentPage.IsThereAnotherPage)
                        {
                            await Database.SetAccountNextMaxIdAsync(account, currentPage.NextPageOptions.MaxID);
                            await DelayByRandom(Options.DelaySeed);
                        }
                    }
                    while (currentPage.IsThereAnotherPage);

                    await Database.DeleteAccountAsync(account);
                }
            }
            catch(InstagramLimitsExceededException ex)
            {
                Console.WriteLine("Hit limit of allowed Instagram calls - message reads: " + ex.Message);
            }
            finally
            {
                Console.WriteLine($"Visited            {accountsVisited} accounts");
                Console.WriteLine($"Liked posts on     {accountsLiked} accounts");
                Console.WriteLine($"Total posts liked: {postsLiked}");
            }
        }

        public bool DoesTextContainAnyHashTags(string text) => string.IsNullOrEmpty(text) ? false : HashTagRegex.IsMatch(text);

        private static Random Randy = new Random();

        /// <summary>
        /// Delays by a random interval that will be 50% higher or lower than the milliseconds provided before calling the
        /// provided function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="millisecondsSeed"></param>
        /// <param name="toDebounce"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static Task DelayByRandom(int millisecondsSeed, CancellationToken token = default) => Task.Delay((millisecondsSeed / 2) + Randy.Next(millisecondsSeed), token);
    }
}
