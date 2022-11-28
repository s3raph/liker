using RestSharp;
using System.Net;
using System.Runtime.Serialization;

namespace Liker.Instagram
{
    [Serializable]
    internal class InstagramRESTException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public RestRequest? Request      { get; }
        public RestResponse? Response    { get; }

        public InstagramRESTException()
        {
        }

        public InstagramRESTException(string message) : base(message)
        {
        }

        public InstagramRESTException(string message, Exception? innerException) : base(message, innerException)
        {
        }

        public InstagramRESTException(string message, HttpStatusCode statusCode, RestRequest request, RestResponse response) : this(message)
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