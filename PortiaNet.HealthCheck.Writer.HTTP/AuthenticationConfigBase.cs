namespace PortiaNet.HealthCheck.Writer.HTTP
{
    public abstract class AuthenticationConfigBase
    {
        internal abstract Task SetAuthorizationHeader(HttpClient client);
    }
}
