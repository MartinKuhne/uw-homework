using Microsoft.AspNetCore.Builder;
using Serilog;

namespace ProductApi.Configuration
{
    public static class LoggingConfigurator
    {
        /// <summary>
        /// Configure Serilog using the application's configuration and wire it into the Host.
        /// This uses the UseSerilog overload that configures Serilog from the HostBuilder context
        /// and avoids assigning to the static Log.Logger directly.
        /// </summary>
        public static void Configure(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((hostContext, services, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);
            });
        }
    }
}
