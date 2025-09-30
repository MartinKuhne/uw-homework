namespace ProductApi.Helpers
{
    public interface ISystem
    {
        DateTimeOffset UtcNow { get; }

        Guid NewGuid { get; }
    }
}