using Ninject.Activation;
using RestSharp;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
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
                //headers.Add("Content-Type"      , "application/x-www-form-urlencoded");
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

        //user_ids=31834762778%2C1391058223%2C208954873%2C1679159388%2C49216558022%2C4723344875%2C2079736750%2C29328037%2C6308439313%2C31535238318%2C1118458899%2C730302

        public async Task<Page<AccountFollower>> GetUserFollowersAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default)
        {
            //var request = new RestRequest($"/api/v1/friendships/45322525656/followers/?count={pageOptions.PageSize}&search_surface=follow_list_page", Method.Get);

            //AddHeadersToRequest(request);

            string userId;

            if (!HandleToUserIDCache.TryGetValue(userHandle, out userId))
            {
                userId = await GetUserProfile(userHandle, cancellationToken);
                HandleToUserIDCache.Add(userHandle, userId);
            }

            var request = new RestRequest($"/api/v1/friendships/{userId}/followers/?count={pageOptions.PageSize}&search_surface=follow_list_page", Method.Get);

            var response = await Client.ExecuteAsync(request, cancellationToken);

            //if (!response.IsSuccessStatusCode)
            //{
            //    throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            //}

            var jsonObj = JsonNode.Parse(response.Content).AsObject();

            var users = jsonObj["users"].Deserialize<Collection<FriendShip>>();

            //if (jsonObj.TryGetPropertyValue("users", out JsonNode? node))
            //{
            //    //node.ToDictionary<string, ExtendedFollowerInfo>((k, v) => k, (k, v) => v);

            //    return node.Deserialize<Collection<FriendShip>>();
            //}

            var statuses = await GetFriendShipStatuses(users.Select(f => f.Pk).ToArray(), cancellationToken);

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
            }).ToList());
        }

        private async Task<string> GetUserProfile(string userName, CancellationToken cancellationToken)
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

                //if (jsonObj.TryGetPropertyValue("friendship_statuses", out JsonNode? node))
                //{
                //    //node.ToDictionary<string, ExtendedFollowerInfo>((k, v) => k, (k, v) => v);

                //    return node.Deserialize<Dictionary<string, ExtendedFollowerInfo>>();
                //}

                return jsonObj["data"]["user"]["id"].GetValue<string>();
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
                    //node.ToDictionary<string, ExtendedFollowerInfo>((k, v) => k, (k, v) => v);

                    return node.Deserialize<Dictionary<string, ExtendedFollowerInfo>>();
                }
            }

            throw new InstagramServiceException($"Failed to {nameof(GetFriendShipStatuses)}");
        }

        public async Task<Page<Post>> GetUserFeedAsync(string userHandle, PageOptions pageOptions, CancellationToken cancellationToken = default)
        {
            //https://www.instagram.com/api/v1/feed/user/penselpelle/username/?count=12
            var request = new RestRequest($"/api/v1/feed/user/{userHandle}/username/?count={pageOptions.PageSize}", Method.Get);

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
                    //node.ToDictionary<string, ExtendedFollowerInfo>((k, v) => k, (k, v) => v);

                    return new Page<Post>(node.Deserialize<List<Post>>());
                }
            }

            throw new InstagramServiceException($"Failed to {nameof(GetUserFeedAsync)}");
        }

        public async Task LikeAsync(int postId, CancellationToken cancellationToken = default)
        {
            //var client = new RestClient("https://www.instagram.com/api/v1/web/likes/2842200996157128449/like/");

            //var client = new RestClient("https://www.instagram.com/");

            var request = new RestRequest($"/api/v1/web/likes/{postId}/like/", Method.Post);

            //AddHeadersToRequest(request);

            var response = await Client.ExecuteAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InstagramServiceException($"Request {request.Resource} failed - Status {response.StatusCode}: {response.StatusDescription}", response.StatusCode, request, response);
            }
        }

        //private void AddHeadersToRequest(RestRequest request)
        //{
        //    //request.AddHeader("User-Agent", USER_AGENT);

        //    //request.AddHeader("X-IG-App-ID", INSTAGRAM_APP_ID);
        //    //request.AddHeader("X-IG-WWW-Claim", "hmac.AR07ZHlDUQP57dosHalfT1Oltkiyzk0vatBe02Rpyo_KoekC");
        //    //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //    //request.AddHeader("X-Requested-With", "XMLHttpRequest");
        //    //request.AddHeader("Origin", "https://www.instagram.com");
        //    //request.AddHeader("Accept", "*/*");
        //    //request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
        //    //request.AddHeader("X-CSRFToken", "cZPpvR2cfqISZ45eGsZ4lkbssMKHkL3H");
        //    //request.AddHeader("Cookie", "csrftoken=cZPpvR2cfqISZ45eGsZ4lkbssMKHkL3H; ds_user_id=49613149133; ig_did=76680F9A-B7C2-4DDF-BA26-5FACFBD75E89; ig_nrcb=1; mid=Y4GsCwALAAHC7gDdoAPxY_-bYwUD; rur=\"EAG\\05449613149133\\0541700979139:01f7c760001a2ae61af13ccac4e74b0b03f3a8ef0e1440bbc076611233629ced05719217\"; sessionid=49613149133%3AU2rbWOt8iu9LwW%3A7%3AAYdPQi6Id8ZD7htnJQS74Pboz7zFxfF8PCrti4xddw; shbid=\"17549\\05449613149133\\0541700978997:01f73fcdff4e7884de6dd9710cfe8e25a3982f4b8bbe2d24c06296d6b40626cbb51ea4a2\"; shbts=\"1669442997\\05449613149133\\0541700978997:01f78ff68ec110707bc1856c55cdaf72c930deb1b3a634c8ca3e55cbc1ced0c83b658fc8\"");

        //    request.AddHeader("X-CSRFToken", Options.CSRFToken);
        //    Client.Add

        //    request.AddHeader("Cookie", $"csrftoken={Options.CSRFToken}; ds_user_id=49613149133; ig_did=76680F9A-B7C2-4DDF-BA26-5FACFBD75E89; ig_nrcb=1; mid=Y4GsCwALAAHC7gDdoAPxY_-bYwUD; rur=\"EAG\\05449613149133\\0541700979139:01f7c760001a2ae61af13ccac4e74b0b03f3a8ef0e1440bbc076611233629ced05719217\"; sessionid={Options.SessionID}; shbid=\"17549\\05449613149133\\0541700978997:01f73fcdff4e7884de6dd9710cfe8e25a3982f4b8bbe2d24c06296d6b40626cbb51ea4a2\"; shbts=\"1669442997\\05449613149133\\0541700978997:01f78ff68ec110707bc1856c55cdaf72c930deb1b3a634c8ca3e55cbc1ced0c83b658fc8\"");
        //}
    }
}
