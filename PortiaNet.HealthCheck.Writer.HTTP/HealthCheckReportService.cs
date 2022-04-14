using PortiaNet.HealthCheck.Reporter;
using PortiaNet.HealthCheck.Writer.HTTP.Authentication;
using System.Text.Json;

namespace PortiaNet.HealthCheck.Writer.HTTP
{
    internal class HealthCheckReportService : IHealthCheckReportService
    {
        private readonly HTTPWriterConfiguration _config;
        private HttpClient? _httpClient;
        private bool _clientConfigMethodHasCalled = false;

        public HealthCheckReportService(HTTPWriterConfiguration config)
        {
            _config = config;
        }

        private void ConfigureHttpClient()
        {
            switch (_config.AuthenticationType)
            {
                case AuthenticationType.None:
                    _httpClient = new HttpClient();
                    break;
                case AuthenticationType.ClientSecretBearerToken:
                case AuthenticationType.UsernamePasswordBearerToken:
                case AuthenticationType.StaticBearerToken:
                    if (_config.AuthenticationConfig == null)
                        throw new ArgumentException($"{nameof(HTTPWriterConfiguration.AuthenticationConfig)} property cannot be null in the selected authentication type.");

                    _httpClient = new HttpClient();
                    _config.AuthenticationConfig.SetAuthorizationHeader(_httpClient).Wait();
                    break;
                default:
                    throw new NotImplementedException($"The authentication type {_config.AuthenticationType} is not implemented.");
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SaveAPICallInformationAsync(RequestDetail requestDetail)
        {
            if (_httpClient == null)
            {
                if (_clientConfigMethodHasCalled)
                    return;

                try
                {
                    ConfigureHttpClient();
                }
                catch
                {
                    _httpClient = null;
                    if (!_config.MuteOnError)
                        throw;

                    return;
                }
                finally
                {
                    _clientConfigMethodHasCalled = true;
                }
            }

            try
            {
                requestDetail.NodeName = _config.NodeName;
                var result = await _httpClient.PostAsync(_config.ListenerAddress,
                    new StringContent(JsonSerializer.Serialize(requestDetail), System.Text.Encoding.UTF8, "application/json"));

                if (!result.IsSuccessStatusCode)
                    throw new AuthenticationFailedException($"Report failed to be sent to the listener with HTTP code {result.StatusCode}.");
            }
            catch
            {
                if(!_config.MuteOnError)
                    throw;
                else
                    return;
            }
        }
    }
}
