using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

public class RequestHeaderLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestHeaderLoggingMiddleware> _logger;

    public RequestHeaderLoggingMiddleware(RequestDelegate next, ILogger<RequestHeaderLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Request.Headers
            .Where(h => h.Key != "Authorization") // Optional: exclude sensitive headers
            .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");

        _logger.LogInformation("Request Headers:\n{Headers}", string.Join("\n", headers));

        await _next(context);
    }
}