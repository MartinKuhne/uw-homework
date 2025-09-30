namespace ProductApi.Helpers
{
    public class SystemImpl : ISystem
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public Guid NewGuid => Guid.NewGuid();
    }
}