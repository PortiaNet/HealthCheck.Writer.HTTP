using PortiaNet.HealthCheck.Writer.HTTP;

namespace PortiaNet.HealthCheck.Writer
{
    public enum AuthenticationType
    {
        None,
        StaticBearerToken,
        ClientSecretBearerToken,
        UsernamePasswordBearerToken
    }

    public class HTTPWriterConfiguration
    {
        /// <summary>
        /// By setting this property to True, all internal exceptions like authentication, and sending request will be suppressed.
        /// </summary>
        public bool MuteOnError { get; set; }

        public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.None;

        public Uri? ListenerAddress { get; set; }

        /// <summary>
        /// This property is required in all <b>AuthenticationType</b>s except <b>None</b>.
        /// </summary>
        public AuthenticationConfigBase? AuthenticationConfig { get; set; }

        public string NodeName { get; set; } = string.Empty;

        /// <summary>
        /// By setting this property to True, the module will send a set of reports to the target machine instead of sending one by one.
        /// </summary>
        public bool BulkDataDumpingEnabled { get; set; } = false;

        /// <summary>
        /// Number of the reports will be sent to the target machine in each dumping process. The default value is 100 items.
        /// </summary>
        public uint DataDumpingSize { get; set; } = 100;
    }
}
