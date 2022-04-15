namespace PortiaNet.HealthCheck.Writer.HTTP.Authentication
{
    public class StaticBearerTokenAuthentication : AuthenticationConfigBase
    {
        public string? Token { get; set; }

        internal override Task SetAuthorizationHeader(HttpClient client)
        {
            if (string.IsNullOrEmpty(Token))
                throw new InvalidDataException("Token property cannot be null.");

            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            return Task.CompletedTask;
        }
    }
}
