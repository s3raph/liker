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
                headers.Add("X-IG-App-ID"       , INSTAGRAM_APP_ID);
                headers.Add("X-IG-WWW-Claim"    , "hmac.AR07ZHlDUQP57dosHalfT1Oltkiyzk0vatBe02Rpyo_KoekC");
                headers.Add("X-Requested-With"  , "XMLHttpRequest");
                headers.Add("Origin"            , "https://www.instagram.com");
                headers.Add("Accept"            , "*/*");
                headers.Add("sec-ch-ua-platform", "\"Windows\"");
            });

        private bool _disposed = false;

        public InstagramService(IInstagramOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            Client.AddDefaultHeader("X-CSRFToken", Options.CSRFToken);
            Client.AddDefaultHeader("Cookie", $"csrftoken={Options.CSRFToken}; ds_user_id=49613149133; ig_did=76680F9A-B7C2-4DDF-BA26-5FACFBD75E89; ig_nrcb=1; mid=Y4GsCwALAAHC7gDdoAPxY_-bYwUD; rur=\"EAG\\05449613149133\\0541700979139:01f7c760001a2ae61af13ccac4e74b0b03f3a8ef0e1440bbc076611233629ced05719217\"; sessionid={Options.SessionID}; shbid=\"17549\\05449613149133\\0541700978997:01f73fcdff4e7884de6dd9710cfe8e25a3982f4b8bbe2d24c06296d6b40626cbb51ea4a2\"; shbts=\"1669442997\\05449613149133\\0541700978997:01f78ff68ec110707bc1856c55cdaf72c930deb1b3a634c8ca3e55cbc1ced0c83b658fc8\"");
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

            var response = await Client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
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
            try
            {
                var request  = new RestRequest($"/api/v1/users/web_profile_info/?username={userName}", Method.Get);
                var response = await Client.ExecuteAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
                }
                else
                {
                    var jsonObj = JsonNode.Parse(response.Content).AsObject();

                    return new UserProfile
                    {
                        UserName      = userName,
                        UserId        = jsonObj["data"]["user"]["id"].GetValue<string>(),
                        FollowerCount = jsonObj["data"]["user"]["edge_followed_by"]["count"].GetValue<int>()
                    };
                }
            }
            catch (Exception)
            {

                throw;
            }

            throw new InstagramServiceException($"Failed to {nameof(GetUserProfile)}");
        }

        private async Task<IReadOnlyDictionary<string, ExtendedFollowerInfo>> GetFriendShipStatuses(string[] userIds, CancellationToken cancellationToken)
        {
            var request = new RestRequest("/api/v1/friendships/show_many/", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddBody($"user_ids={string.Join("%2C", userIds)}");
            var response = await Client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }
            else
            {
                var jsonObj = JsonNode.Parse(response.Content).AsObject();

                if (jsonObj.TryGetPropertyValue("friendship_statuses", out JsonNode? node))
                {
                    return node.Deserialize<Dictionary<string, ExtendedFollowerInfo>>();
                }
            }

            throw new InstagramServiceException($"Failed to {nameof(GetFriendShipStatuses)}");
        }

        public async Task<Page<Post>> GetUserFeedAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/api/v1/feed/user/{userHandle}/username/?{pageOptions.AsQueryString()}", Method.Get);

            var response = await Client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
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

            throw new InstagramServiceException($"Failed to {nameof(GetUserFeedAsync)}");
        }

        public async Task LikeAsync(string postId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/api/v1/web/likes/{postId}/like/", Method.Post);

            var response = await Client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }
        }
    }
}
