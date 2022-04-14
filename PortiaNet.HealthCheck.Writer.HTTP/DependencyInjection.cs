using Microsoft.Extensions.DependencyInjection;
using PortiaNet.HealthCheck.Reporter;
using PortiaNet.HealthCheck.Writer.HTTP;

namespace PortiaNet.HealthCheck.Writer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHTTPWriter(this IServiceCollection services, Action<HTTPWriterConfiguration> configuration)
        {
            var config = new HTTPWriterConfiguration();
            configuration(config);
            var reportServiceImplementation = new HealthCheckReportService(config);
            services.AddSingleton<IHealthCheckReportService>(reportServiceImplementation);
            return services;
        }
    }
}
