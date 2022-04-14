using System.Runtime.Serialization;

namespace PortiaNet.HealthCheck.Writer.HTTP.Authentication
{
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException()
        {
        }

        public AuthenticationFailedException(string? message) : base(message)
        {
        }

        public AuthenticationFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AuthenticationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
