using RestSharp;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Liker.Instagram
{
    public class InstagramService : IInstagramService, IDisposable
    {
        private const string USER_AGENT       = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.35";
        private const string INSTAGRAM_APP_ID = "936619743392459";

        private readonly Dictionary<string, string> HandleToUserIDCache = new Dictionary<string, string>();
        private readonly IInstagramOptions Options;
        private readonly RestClient Client = new RestClient(
            new RestClientOptions("https://www.instagram.com/")
            {
                UserAgent = USER_AGENT
            },
            headers =>
            {
                // Client
                headers.Add("Accept"            , "*/*");
                headers.Add("Accept-Language"   , "en-GB,en;q=0.9,en-US;q=0.8");
                headers.Add("X-Requested-With"  , "XMLHttpRequest");

                // Miscellaneous
                headers.Add("X-ASBD-ID"         , "198387");
                headers.Add("X-IG-App-ID"       , INSTAGRAM_APP_ID);

                // Security
                headers.Add("Origin"                     , "https://www.instagram.com");
                headers.Add("sec-ch-prefers-color-scheme", "light");
                headers.Add("sec-ch-ua"                  , "\"Microsoft Edge\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
                headers.Add("sec-ch-ua-mobile"           , "?0");
                headers.Add("sec-ch-ua-platform"         , "\"Windows\"");
                headers.Add("Sec-Fetch-Dest"             , "empty");
                headers.Add("Sec-Fetch-Mode"             , "cors");
                headers.Add("Sec-Fetch-Site"             , "same-origin");
            });

        private bool _disposed            = false;
        private int _userProfileCallsMade = 0;

        public InstagramService(IInstagramOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            Client.AddDefaultHeader("X-CSRFToken"     , Options.CSRFToken);
            Client.AddDefaultHeader("Cookie"          , $"csrftoken={Options.CSRFToken}; ds_user_id=49613149133; ig_did=76680F9A-B7C2-4DDF-BA26-5FACFBD75E89; ig_nrcb=1; mid=Y4GsCwALAAHC7gDdoAPxY_-bYwUD; rur=\"EAG\\05449613149133\\0541700979139:01f7c760001a2ae61af13ccac4e74b0b03f3a8ef0e1440bbc076611233629ced05719217\"; sessionid={Options.SessionID}; shbid=\"17549\\05449613149133\\0541700978997:01f73fcdff4e7884de6dd9710cfe8e25a3982f4b8bbe2d24c06296d6b40626cbb51ea4a2\"; shbts=\"1669442997\\05449613149133\\0541700978997:01f78ff68ec110707bc1856c55cdaf72c930deb1b3a634c8ca3e55cbc1ced0c83b658fc8\"");
            Client.AddDefaultHeader("X-IG-WWW-Claim"  , Options.IGWWWClaim);
            Client.AddDefaultHeader("X-Instagram-AJAX", Options.IGAjax);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Client.Dispose();
            }
        }

        public async Task<Page<AccountFollower>> GetUserFollowersAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default)
        {
            string userId;

            if (!HandleToUserIDCache.TryGetValue(userHandle, out userId))
            {
                userId = (await GetUserProfile(userHandle, cancellationToken)).UserId;
                HandleToUserIDCache.Add(userHandle, userId);
            }

            var request = new RestRequest($"/api/v1/friendships/{userId}/followers/?{pageOptions.AsQueryString()}&search_surface=follow_list_page", Method.Get);

            var response = await Client.ExecuteAsync(request);

            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramRESTException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }

            var jsonObj = JsonNode.Parse(response.Content).AsObject();
            var users   = jsonObj["users"].Deserialize<Collection<FriendShip>>();

            var statuses = await GetFriendShipStatuses(users.Select(f => f.Pk).ToArray(), cancellationToken);

            if (jsonObj.TryGetPropertyValue("next_max_id", out var nextMaxNode))
            {
                return new Page<AccountFollower>(users.Select(f =>
                {
                    var status = statuses[f.Pk];
                    return new AccountFollower
                    {
                        UserID       = f.Pk,
                        Username     = f.Username,
                        IsPrivate    = f.IsPrivate,
                        Following    = status.Following,
                        IsRestricted = status.IsRestricted
                    };
                }).ToList())
                {
                    IsThereAnotherPage = true,
                    NextPageOptions    = new PageOptions { MaxID = nextMaxNode.GetValue<string>() }
                };
            }
            else
            {
                return new Page<AccountFollower>(users.Select(f =>
                {
                    var status = statuses[f.Pk];
                    return new AccountFollower
                    {
                        UserID       = f.Pk,
                        Username     = f.Username,
                        IsPrivate    = f.IsPrivate,
                        Following    = status.Following,
                        IsRestricted = status.IsRestricted
                    };
                }).ToList())
                {
                    IsThereAnotherPage = false
                };
            }
        }

        public async Task<UserProfile> GetUserProfile(string userName, CancellationToken cancellationToken)
        {
            if (++_userProfileCallsMade > Options.MaxAllowedUserProfileInfoCalls)
            {
                throw new InstagramLimitsExceededException($"Max allowed calls to /api/v1/users/web_profile_info/ exceeded - threshold set at {Options.MaxAllowedUserProfileInfoCalls}");
            }

            // Got 429 too many requests response on this after a couple of hours - I suspect the rough volume is >500 requests
            try
            {
                var request  = new RestRequest($"/api/v1/users/web_profile_info/?username={userName}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                cancellationToken.ThrowIfCancellationRequested();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InstagramRESTException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
                }
                else
                {
                    var jsonObj = JsonNode.Parse(response.Content).AsObject();

                    return new UserProfile
                    {
                        UserName         = userName,
                        UserId           = jsonObj["data"]["user"]["id"].GetValue<string>(),
                        FollowedByViewer = jsonObj["data"]["user"]["followed_by_viewer"].GetValue<bool>(),
                        FollowsViewer    = jsonObj["data"]["user"]["follows_viewer"].GetValue<bool>(),
                        HasBlockedViewer = jsonObj["data"]["user"]["has_blocked_viewer"].GetValue<bool>(),
                        FollowerCount    = jsonObj["data"]["user"]["edge_followed_by"]["count"].GetValue<int>()
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

            throw new InstagramRESTException($"Failed to {nameof(GetUserProfile)}");
        }

        private async Task<IReadOnlyDictionary<string, ExtendedFollowerInfo>> GetFriendShipStatuses(string[] userIds, CancellationToken cancellationToken)
        {
            var request = new RestRequest("/api/v1/friendships/show_many/", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddBody($"user_ids={string.Join("%2C", userIds)}");
            var response = await Client.ExecuteAsync(request);

            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramRESTException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }
            else
            {
                var jsonObj = JsonNode.Parse(response.Content).AsObject();

                if (jsonObj.TryGetPropertyValue("friendship_statuses", out JsonNode? node))
                {
                    return node.Deserialize<Dictionary<string, ExtendedFollowerInfo>>();
                }
            }

            throw new InstagramRESTException($"Failed to {nameof(GetFriendShipStatuses)}");
        }

        public async Task<Page<Post>> GetUserFeedAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/api/v1/feed/user/{userHandle}/username/?{pageOptions.AsQueryString()}", Method.Get);

            var response = await Client.ExecuteAsync(request);

            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramRESTException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }
            else
            {
                var jsonObj = JsonNode.Parse(response.Content).AsObject();

                if (jsonObj.TryGetPropertyValue("items", out JsonNode? node))
                {
                    if (jsonObj.TryGetPropertyValue("next_max_id", out var nextMaxNode))
                    {
                        return new Page<Post>(node.Deserialize<List<Post>>())
                        {
                            IsThereAnotherPage = true,
                            NextPageOptions    = new PageOptions { MaxID = nextMaxNode.GetValue<string>() }
                        };
                    }
                    else
                    {
                        return new Page<Post>(node.Deserialize<List<Post>>())
                        {
                            IsThereAnotherPage = false
                        };
                    }
                }
            }

            throw new InstagramRESTException($"Failed to {nameof(GetUserFeedAsync)}");
        }

        public async Task LikeAsync(string postId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/api/v1/web/likes/{postId}/like/", Method.Post);

            var response = await Client.ExecuteAsync(request);

            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramRESTException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }
        }
    }
}
