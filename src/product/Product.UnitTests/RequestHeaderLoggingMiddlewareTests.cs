using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RequestHeaderLoggingMiddlewareTests
{
    private class InMemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentBag<string> _messages = new ConcurrentBag<string>();

        public ILogger CreateLogger(string categoryName) => new ProviderLogger(_messages);

        public void Dispose() { }

        public IEnumerable<string> Messages => _messages.ToArray();

        private class ProviderLogger : ILogger
        {
            private readonly ConcurrentBag<string> _messages;

            public ProviderLogger(ConcurrentBag<string> messages)
            {
                _messages = messages;
            }

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception? exception, System.Func<TState, System.Exception?, string> formatter)
            {
                _messages.Add(formatter(state, exception));
            }
        }
    }

    [Test]
    public async Task Middleware_DoesNot_Log_Authorization_Header()
    {
        // Arrange
        var provider = new InMemoryLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var logger = factory.CreateLogger<RequestHeaderLoggingMiddleware>();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/test";
        context.Request.Headers["Authorization"] = "Bearer secret-token";
        context.Request.Headers["X-Another-Header"] = "Value";

        // Create a terminal delegate that does nothing
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;

    var middleware = new RequestHeaderLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - ensure Authorization is not present, but other header is
        var messages = string.Join("\n", provider.Messages);
        Assert.IsTrue(!string.IsNullOrEmpty(messages), "Expected at least one log message");
        StringAssert.DoesNotContain("Authorization:", messages);
        StringAssert.Contains("X-Another-Header: Value", messages);
    }

    [Test]
    public async Task Middleware_Logs_Request_Headers()
    {
        // Arrange
        var provider = new InMemoryLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var logger = factory.CreateLogger<RequestHeaderLoggingMiddleware>();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/test";
        context.Request.Headers["X-Test-Header"] = "HeaderValue";

        // Create a terminal delegate that does nothing
        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;

    var middleware = new RequestHeaderLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var messages = string.Join("\n", provider.Messages);
        Assert.IsTrue(!string.IsNullOrEmpty(messages), "Expected at least one log message");
        StringAssert.Contains("X-Test-Header: HeaderValue", messages);
    }
}
