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

        protected InstagramServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an error occurs while interacting with the Instagram API.
    /// </summary>
    [Serializable]
    internal class InstagramRESTException : InstagramServiceException
    {
        public HttpStatusCode StatusCode { get; }
        public RestRequest? Request      { get; }
        public RestResponse? Response    { get; }

        private InstagramRESTException() : base(string.Empty) { }

        public InstagramRESTException(string message, HttpStatusCode statusCode, RestRequest request, RestResponse response) : base(message)
        {
            StatusCode = statusCode;
            Request    = request;
            Response   = response;
        }

        protected InstagramRESTException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}