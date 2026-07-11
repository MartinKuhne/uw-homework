using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Configuration;

namespace ProductApi.Configuration
{
    public static class JwtConfigurator
    {
        public static void ConfigureJwt(WebApplicationBuilder builder, JwtOptions jwtOptions)
        {
            // If an authority/metadata is configured, enable JWT Bearer authentication
            if (!string.IsNullOrWhiteSpace(jwtOptions.Authority) || !string.IsNullOrWhiteSpace(jwtOptions.MetadataAddress))
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    if (!string.IsNullOrWhiteSpace(jwtOptions.MetadataAddress))
                    {
                        options.MetadataAddress = jwtOptions.MetadataAddress;
                    }
                    else if (!string.IsNullOrWhiteSpace(jwtOptions.Authority))
                    {
                        options.Authority = jwtOptions.Authority;
                    }

                    options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;

                    // Ensure token validation parameters exist and explicitly disable audience validation
                    options.TokenValidationParameters ??= new Microsoft.IdentityModel.Tokens.TokenValidationParameters();
                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidateAudience = false; // explicitly turn off audience validation
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;

                    // We may still set an audience for other purposes, but validation will remain disabled
                    if (!string.IsNullOrWhiteSpace(jwtOptions.Audience))
                    {
                        options.Audience = jwtOptions.Audience;
                    }
                });

                // NOTE: Do not add Authorization here; we'll register policies below.
            }

            // Register authorization and the WriteScopePolicy (fallback to RequireAuthenticatedUser when no scope configured)
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("WriteScopePolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    if (!string.IsNullOrWhiteSpace(jwtOptions.WriteScope))
                    {
                        policy.RequireAssertion(context =>
                        {
                            var scopeClaim = context.User.FindFirst(c => c.Type == "scope" || c.Type == "scp");
                            if (scopeClaim == null)
                            {
                                return false;
                            }
                            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            return scopes.Contains(jwtOptions.WriteScope);
                        });
                    }
                    // When WriteScope not configured, policy simply requires authenticated user
                });
            });
        }
    }
}
