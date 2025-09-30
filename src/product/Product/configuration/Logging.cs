using Microsoft.AspNetCore.Builder;
using Serilog;

namespace ProductApi.Configuration
{
    public static class LoggingConfigurator
    {
        /// <summary>
        /// Configure Serilog using the application's configuration and wire it into the Host.
        /// </summary>
        public static void Configure(WebApplicationBuilder builder)
        {
            // Configure Serilog from configuration (appsettings.json)
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            // Use Serilog for the host logging
            builder.Host.UseSerilog();
        }
    }
}
