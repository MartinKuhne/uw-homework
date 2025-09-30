namespace ProductApi.Configuration;

public class JwtOptions
{
    // Authority (issuer) for the OpenID Connect provider (e.g. https://auth.example.com)
    public string? Authority { get; set; }

    // Optional explicit metadata address (well-known configuration endpoint)
    public string? MetadataAddress { get; set; }

    // Optional expected audience for tokens
    public string? Audience { get; set; }

    // Whether to require HTTPS for metadata retrieval
    public bool RequireHttpsMetadata { get; set; } = true;

    // Optional write scope name to require for admin/write endpoints (checked in 'scope' or 'scp' claims)
    public string? WriteScope { get; set; }
}
