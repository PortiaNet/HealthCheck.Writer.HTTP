using System.Text.Json;

namespace PortiaNet.HealthCheck.Writer.HTTP.Authentication
{
    public class UsernamePasswordBearerTokenAuthentication : AuthenticationConfigBase
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        /// <summary>
        /// The POST authentication method that accepts the JSONized <b>ClientSecret</b> in the request body and returns the authorization token
        /// The request body will be like the following:
        /// {
        ///     "Username": "VALUE OF Username PROPERTY",
        ///     "Password": "VALUE OF THE Password PROPERTY"
        /// }
        /// </summary>
        public Uri? AuthenticationAPIPath { get; set; }

        internal override async Task SetAuthorizationHeader(HttpClient client)
        {
            if (string.IsNullOrEmpty(Username))
                throw new InvalidDataException($"{nameof(Username)} property cannot be null.");

            if (string.IsNullOrEmpty(Password))
                throw new InvalidDataException($"{nameof(Password)} property cannot be null.");

            if (AuthenticationAPIPath == null)
                throw new InvalidDataException($"{nameof(AuthenticationAPIPath)} property cannot be null.");

            if (client == null)
                throw new ArgumentNullException(nameof(client));

            using var authenticationClient = new HttpClient();
            authenticationClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var result = await authenticationClient.PostAsync(AuthenticationAPIPath,
                new StringContent(JsonSerializer.Serialize(new { Username, Password }), System.Text.Encoding.UTF8, "application/json"));

            if (result.IsSuccessStatusCode)
            {
                var response = JsonSerializer.Deserialize<string>(await result.Content.ReadAsStringAsync());
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response);
            }
            else
                throw new AuthenticationFailedException($"Authentication failed with HTTP status {result.StatusCode}");
        }
    }
}
