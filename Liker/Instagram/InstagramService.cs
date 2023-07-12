using RestSharp;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Liker.Instagram
{
    /// <inheritdoc/>
    public class InstagramService : IInstagramService, IDisposable
    {
        private const string USER_AGENT       = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.35";
        private const string INSTAGRAM_APP_ID = "936619743392459";

        private readonly Dictionary<string, int> NumberOfCallsMadeByUri = new();
        private readonly Dictionary<string, string> HandleToUserIDCache = new();
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

        private bool _disposed = false;

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

        /// <inheritdoc/>
        [Retry]
        public async Task<Page<AccountFollower>> GetUserFollowersAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default)
        {
            string userId;

            if (!HandleToUserIDCache.TryGetValue(userHandle, out userId))
            {
                userId = (await GetUserProfile(userHandle, cancellationToken)).UserId;
                HandleToUserIDCache.Add(userHandle, userId);
            }

            var friendshipsPage = await InvokeRequest(
                new RestRequest($"/api/v1/friendships/{userId}/followers/?{pageOptions.AsQueryString()}&search_surface=follow_list_page", Method.Get),
                response => DeserializePage<FriendShip>(response.RawBytes, "users"));

            var statuses = friendshipsPage.Any() ? await GetFriendShipStatuses(friendshipsPage.Select(f => f.Pk).ToArray(), cancellationToken) : new Dictionary<string, ExtendedFollowerInfo>();

            return new Page<AccountFollower>(friendshipsPage.Select(f =>
            {
                statuses.TryGetValue(f.PkId, out var status);

                return new AccountFollower
                {
                    UserID       = f.Pk,
                    Username     = f.Username,
                    IsPrivate    = f.IsPrivate,
                    Following    = status?.Following,
                    IsRestricted = status?.IsRestricted
                };
            }).ToList())
            {
                NextPageOptions = friendshipsPage.NextPageOptions
            };
        }

        /// <inheritdoc/>
        [Retry]
        public Task<UserProfile> GetUserProfile(string userName, CancellationToken cancellationToken) =>
            InvokeRequest(
                new RestRequest($"/api/v1/users/web_profile_info/?username={userName}", Method.Get),
                response =>
                {
                    var jsonObj = ParseJsonObject(response.RawBytes);

                    return new UserProfile
                    {
                        UserName         = userName,
                        UserId           = jsonObj["data"]["user"]["id"].GetValue<string>(),
                        FollowedByViewer = jsonObj["data"]["user"]["followed_by_viewer"].GetValue<bool>(),
                        FollowsViewer    = jsonObj["data"]["user"]["follows_viewer"].GetValue<bool>(),
                        HasBlockedViewer = jsonObj["data"]["user"]["has_blocked_viewer"].GetValue<bool>(),
                        FollowerCount    = jsonObj["data"]["user"]["edge_followed_by"]["count"].GetValue<int>()
                    };
                });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InstagramServiceException"></exception>
        private Task<IReadOnlyDictionary<string, ExtendedFollowerInfo>> GetFriendShipStatuses(string[] userIds, CancellationToken cancellationToken)
        {
            if (!userIds.Any()) return Task.FromResult<IReadOnlyDictionary<string, ExtendedFollowerInfo>>(new Dictionary<string, ExtendedFollowerInfo>());

            var request = new RestRequest("/api/v1/friendships/show_many/", Method.Post);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddBody($"user_ids={string.Join("%2C", userIds)}");

            return InvokeRequest<IReadOnlyDictionary<string, ExtendedFollowerInfo>>(
                request,
                response =>
                {
                    if (ParseJsonObject(response.RawBytes).TryGetPropertyValue("friendship_statuses", out JsonNode? node))
                    {
                        return node.Deserialize<Dictionary<string, ExtendedFollowerInfo>>();
                    }
                    else
                    {
                        throw new InstagramServiceException($"Failed to {nameof(GetFriendShipStatuses)}");
                    }
                });
        }

        /// <inheritdoc/>
        public Task<Page<Post>> GetUserFeedAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default) =>
            InvokeRequest(
                new RestRequest($"/api/v1/feed/user/{userHandle}/username/?{pageOptions.AsQueryString()}", Method.Get),
                response => DeserializePage<Post>(response.RawBytes, "items"));

        /// <inheritdoc/>
        public async Task LikeAsync(string postId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"/api/v1/web/likes/{postId}/like/", Method.Post);

            var responseTask = Client.ExecuteAsync(request);

            IncrementRequestCountForResource(request);

            var response = await responseTask;

            if (!response.IsSuccessStatusCode)
            {
                throw MakeInstagramExceptionFromResponse(response);
            }
        }

        private async Task<T> InvokeRequest<T>(RestRequest request, Func<RestResponse, T> resultProcessing)
        {
            var responseTask = Client.ExecuteAsync(request);

            IncrementRequestCountForResource(request);

            var response = await responseTask;

            if (!response.IsSuccessStatusCode)
            {
                throw MakeInstagramExceptionFromResponse(response);
            }
            else
            {
                return resultProcessing(response);
            }
        }

        /// <summary>
        /// Helper method that checks if NumberOfCallsMadeByUri contains the given
        /// Resource and if not adds it.
        /// </summary>
        /// <param name="request"></param>
        private void IncrementRequestCountForResource(RestRequest request)
        {
            if (NumberOfCallsMadeByUri.TryGetValue(request.Resource, out int callCount))
            {
                NumberOfCallsMadeByUri[request.Resource] = callCount + 1;
            }
            else
            {
                NumberOfCallsMadeByUri[request.Resource] = 1;
            }
        }

        /// <summary>
        /// Helper method that ensures the appropriate exception type, enriched with the relevant
        /// supporting information is thrown for a given HTTP status code.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private InstagramServiceException MakeInstagramExceptionFromResponse(RestResponse response)
        {
            var request = response.Request ?? throw new ArgumentOutOfRangeException(nameof(response), $"{nameof(response.Request)} must not be null");

            switch (response.StatusCode)
            {
                case HttpStatusCode.TooManyRequests:
                    return new InstagramRESTLimitsExceededException(response, NumberOfCallsMadeByUri[request.Resource]);
                default:
                    return new InstagramRESTException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response);
            }
        }

        /// <summary>
        /// Takes raw response content and parses a <see cref="Page{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawBytes"></param>
        /// <param name="collectionPropertyName"></param>
        /// <param name="nextMaxIdPropertyName"></param>
        /// <returns></returns>
        /// <exception cref="InstagramServiceException"></exception>
        private static Page<T> DeserializePage<T>(ReadOnlySpan<byte> rawBytes, string collectionPropertyName, string nextMaxIdPropertyName = "next_max_id")
        {
            JsonObject rootObj = ParseJsonObject(rawBytes);

            if (rootObj.TryGetPropertyValue(collectionPropertyName, out JsonNode? node) && node is JsonArray)
            {
                var items = node.Deserialize<List<T>>() ?? new List<T>();

                if (rootObj.TryGetPropertyValue(nextMaxIdPropertyName, out var nextMaxNode) && nextMaxNode is JsonNode)
                {
                    return new Page<T>(items)
                    {
                        NextPageOptions = new PageOptions { MaxID = nextMaxNode.GetValue<string>() }
                    };
                }
                else
                {
                    return new Page<T>(items);
                }
            }

            throw new InstagramServiceException("Failed to deserialize page");
        }

        private static JsonObject ParseJsonObject(ReadOnlySpan<byte> rawBytes)
        {
            var rootNode = JsonNode.Parse(rawBytes);
            var rootObj  = rootNode?.AsObject() ?? throw new InstagramServiceException($"Failed to parse root JsonObject from {nameof(rawBytes)}");
            return rootObj;
        }
    }
}
