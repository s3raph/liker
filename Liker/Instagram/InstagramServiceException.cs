using RestSharp;
using System.Net;
using System.Runtime.Serialization;

namespace Liker.Instagram
{
    /// <summary>
    /// Exception thrown when an error occurs within the <see cref="InstagramService"/>
    /// </summary>
    [Serializable]
    internal class InstagramServiceException : ApplicationException
    {
        public InstagramServiceException(string message) : base(message) { }

        protected InstagramServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when an error occurs while interacting with the Instagram API.
    /// </summary>
    [Serializable]
    internal class InstagramRESTException : InstagramServiceException
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string? StatusDescription { get; private set; }
        public Uri? ResponseUri          { get; private set; }


        public InstagramRESTException(RestResponse response)
            : base($"Received {response.StatusCode} ({response.StatusDescription}) from request to {response.ResponseUri}")
        {
            StatusCode        = response.StatusCode;
            StatusDescription = response.StatusDescription;
            ResponseUri       = response.ResponseUri;
        }

        public InstagramRESTException(string message, RestResponse response)
            : base(message)
        {
            StatusCode        = response.StatusCode;
            StatusDescription = response.StatusDescription;
            ResponseUri       = response.ResponseUri;
        }

        protected InstagramRESTException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info.GetValue(nameof(StatusCode), typeof(HttpStatusCode)) is HttpStatusCode stat)
            {
                StatusCode = stat;
            }

            StatusDescription = info.GetString(nameof(StatusDescription));
            ResponseUri       = info.GetValue (nameof(ResponseUri), typeof(Uri)) as Uri;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(StatusCode)       , StatusCode);
            info.AddValue(nameof(StatusDescription), StatusDescription);
            info.AddValue(nameof(ResponseUri)      , ResponseUri);

            // MUST call through to the base class to let it save its own state
            base.GetObjectData(info, context);
        }
    }
}