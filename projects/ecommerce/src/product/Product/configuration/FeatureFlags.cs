namespace ProductApi.Configuration;

public class FeatureFlags
{
    public bool EnableAdminApi { get; set; } = true;
    public bool EnableCatalogApi { get; set; } = true;
}