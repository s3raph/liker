using System.Runtime.Serialization;

namespace Liker.Instagram
{
    [Serializable]
    internal class InstagramLimitsExceededException : Exception
    {
        public InstagramLimitsExceededException()
        {
        }

        public InstagramLimitsExceededException(string? message) : base(message)
        {
        }

        public InstagramLimitsExceededException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InstagramLimitsExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}