using RestSharp;
using System.Net;
using System.Runtime.Serialization;

namespace Liker.Instagram
{
    [Serializable]
    internal class InstagramServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public RestRequest? Request      { get; }
        public RestResponse? Response    { get; }

        public InstagramServiceException()
        {
        }

        public InstagramServiceException(string message) : base(message)
        {
        }

        public InstagramServiceException(string message, Exception? innerException) : base(message, innerException)
        {
        }

        public InstagramServiceException(string message, HttpStatusCode statusCode, RestRequest request, RestResponse response) : this(message)
        {
            StatusCode = statusCode;
            Request    = request;
            Response   = response;
        }

        protected InstagramServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}