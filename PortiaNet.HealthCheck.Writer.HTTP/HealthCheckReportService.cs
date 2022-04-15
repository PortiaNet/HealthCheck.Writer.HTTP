using PortiaNet.HealthCheck.Reporter;
using PortiaNet.HealthCheck.Writer.HTTP.Authentication;
using System.Diagnostics;
using System.Text.Json;

namespace PortiaNet.HealthCheck.Writer.HTTP
{
    internal class HealthCheckReportService : IHealthCheckReportService
    {
        private readonly HTTPWriterConfiguration _config;
        private HttpClient? _httpClient;
        private bool _clientConfigMethodHasCalled = false;
        private readonly Queue<RequestDetail> _dumpingQueue = new();

        public HealthCheckReportService(HTTPWriterConfiguration config)
        {
            _config = config;
            _config.DataDumpingSize = Math.Max(1, _config.DataDumpingSize);
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

        public Task SaveAPICallInformationAsync(RequestDetail requestDetail)
        {
            if (_httpClient == null)
            {
                if (_clientConfigMethodHasCalled)
                    return Task.CompletedTask;

                try
                {
                    ConfigureHttpClient();
                }
                catch (Exception ex)
                {
                    Debugger.Log(0, "HTTP Writer", Environment.NewLine);
                    Debugger.Log(0, "HTTP Writer", $"Authentication Error :: {ex.Message}");
                    Debugger.Log(0, "HTTP Writer", Environment.NewLine);
                    Debugger.Log(0, "HTTP Writer", ex.StackTrace);
                    Debugger.Log(0, "HTTP Writer", Environment.NewLine);

                    _httpClient = null;
                    if (!_config.MuteOnError)
                        throw;

                    return Task.CompletedTask;
                }
                finally
                {
                    _clientConfigMethodHasCalled = true;
                }
            }

            try
            {
                requestDetail.NodeName = _config.NodeName;

                if (_config.BulkDataDumpingEnabled)
                {
                    _dumpingQueue.Enqueue(requestDetail);
                    if (_dumpingQueue.Count >= _config.DataDumpingSize)
                    {
                        var itemsToSend = new List<RequestDetail>();
                        var index = 0;
                        while (index < _config.DataDumpingSize && _dumpingQueue.Count > 0)
                        {
                            itemsToSend.Add(_dumpingQueue.Dequeue());
                            index++;
                        }

                        return PostInformation(JsonSerializer.Serialize(itemsToSend));
                    }

                    return Task.CompletedTask;
                }
                else
                    return PostInformation(JsonSerializer.Serialize(requestDetail));
            }
            catch (Exception ex)
            {
                Debugger.Log(0, "HTTP Writer", Environment.NewLine);
                Debugger.Log(0, "HTTP Writer", $"Error :: {ex.Message}");
                Debugger.Log(0, "HTTP Writer", Environment.NewLine);
                Debugger.Log(0, "HTTP Writer", ex.StackTrace);
                Debugger.Log(0, "HTTP Writer", Environment.NewLine);

                if (!_config.MuteOnError)
                    throw;
                else
                    return Task.CompletedTask;
            }
        }

        private async Task PostInformation(string jsonifiedContent)
        {
            var result = await _httpClient.PostAsync(_config.ListenerAddress,
                    new StringContent(jsonifiedContent, System.Text.Encoding.UTF8, "application/json"));

            if (!result.IsSuccessStatusCode)
                throw new AuthenticationFailedException($"Report failed to be sent to the listener with HTTP code {result.StatusCode}.");
        }
    }
}
