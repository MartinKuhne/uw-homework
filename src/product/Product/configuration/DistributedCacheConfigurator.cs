using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProductApi.Configuration
{
    /// <summary>
    /// Registers a distributed cache implementation for the application.
    /// Prefers Redis when configured (Redis:ConnectionString or REDIS__CONNECTIONSTRING),
    /// otherwise falls back to an in-memory distributed cache suitable for local/dev/test.
    /// </summary>
    public static class DistributedCacheConfigurator
    {
        public static void ConfigureDistributedCache(WebApplicationBuilder builder)
        {
            // Configuration keys checked (in order): Redis:ConnectionString, REDIS__CONNECTIONSTRING (via env vars)
            var redisConn = builder.Configuration.GetSection("Redis").GetValue<string>("ConnectionString");
            if (string.IsNullOrWhiteSpace(redisConn))
            {
                redisConn = builder.Configuration.GetValue<string>("REDIS__CONNECTIONSTRING");
            }

            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                // Use StackExchange.Redis based distributed cache
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConn;
                });
            }
            else
            {
                // Fallback to in-memory distributed cache for local/dev/testing
                builder.Services.AddDistributedMemoryCache();
            }
        }
    }
}
