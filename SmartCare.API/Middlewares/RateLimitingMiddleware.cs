using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class RateLimitingOptions
{
    public int RequestsPerWindow { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _limit;
    private readonly TimeSpan _window;
    private static readonly ConcurrentDictionary<string, (int Count, DateTime Reset)> _requests = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _logger = logger;
        _limit = options.Value.RequestsPerWindow;
        _window = TimeSpan.FromSeconds(options.Value.WindowSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var (count, reset) = _requests.GetOrAdd(ip, _ => (0, now.Add(_window)));

        if (now > reset)
        {
            _requests[ip] = (1, now.Add(_window));
        }
        else
        {
            if (count >= _limit)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IP}", ip);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = ((int)(_window.TotalSeconds)).ToString();
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            _requests[ip] = (count + 1, reset);
        }

        await _next(context);
    }
}
