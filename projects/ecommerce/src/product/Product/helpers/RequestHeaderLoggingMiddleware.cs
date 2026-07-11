using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        // Build a structured dictionary of headers (excluding sensitive ones)
        var headersDict = context.Request.Headers
            .Where(h => !string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.Count > 0 ? h.Value.ToArray() : Array.Empty<string>());

        // Structured log: use destructuring to preserve the header dictionary shape
        _logger.LogInformation("Request {Method} {Path} headers: {@Headers}", context.Request.Method, context.Request.Path, headersDict);

        await _next(context);
    }
}