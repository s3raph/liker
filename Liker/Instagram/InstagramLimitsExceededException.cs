using RestSharp;
using System.Runtime.Serialization;

namespace Liker.Instagram
{
    /// <summary>
    /// The exception that is thrown when Instagram limits are exceeded and a HTTP 429 response is received.
    /// </summary>
    [Serializable]
    internal class InstagramRESTLimitsExceededException : InstagramRESTException
    {
        /// <summary>
        /// The number of calls made by the instance of <see cref="InstagramService"/> to <see cref="PathExceeded"/> when the 429 response was received.
        /// </summary>
        public int CallsMade { get; private set; }

        public InstagramRESTLimitsExceededException(RestResponse response, int callsMade)
            : base($"Instagram limits exceeded for {response.ResponseUri}. {callsMade} calls made.", response)
        {
            CallsMade = callsMade;
        }

        public InstagramRESTLimitsExceededException(string message, RestResponse response, int callsMade)
            : base(message, response)
        {
            CallsMade = callsMade;
        }

        protected InstagramRESTLimitsExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CallsMade = info.GetInt32(nameof(CallsMade));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(CallsMade), CallsMade);

            // MUST call through to the base class to let it save its own state
            base.GetObjectData(info, context);
        }
    }
}