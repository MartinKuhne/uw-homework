using ProductApi.Helpers;

namespace ProductApi.UnitTests.TestHelpers
{
    /// <summary>
    /// Simple deterministic ISystem implementation for tests.
    /// </summary>
    public sealed class FakeSystem : ISystem
    {
        public FakeSystem(Guid id, DateTimeOffset now)
        {
            GuidValue = id;
            Now = now;
        }

        private Guid GuidValue { get; }
        private DateTimeOffset Now { get; }

        public DateTimeOffset UtcNow => Now;

        public Guid NewGuid => GuidValue;
    }
}
