using Microsoft.AspNetCore.Http;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

public class RequestHeaderLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestHeaderLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Request.Headers
            .Where(h => h.Key != "Authorization") // Optional: exclude sensitive headers
            .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");

        Log.Information("Request Headers:\n{Headers}", string.Join("\n", headers));

        await _next(context);
    }
}