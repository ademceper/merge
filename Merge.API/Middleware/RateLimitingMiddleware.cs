using System.Collections.Concurrent;
using System.Net;

namespace Merge.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clientRequests = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitAttribute = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();

        if (rateLimitAttribute != null)
        {
            var clientId = GetClientIdentifier(context);
            var requestInfo = _clientRequests.GetOrAdd(clientId, _ => new ClientRequestInfo());

            bool rateLimitExceeded;
            lock (requestInfo)
            {
                var now = DateTime.UtcNow;

                // Clean up old requests
                requestInfo.RequestTimes.RemoveAll(t => now - t > rateLimitAttribute.TimeWindow);

                // Check if limit is exceeded
                if (requestInfo.RequestTimes.Count >= rateLimitAttribute.MaxRequests)
                {
                    rateLimitExceeded = true;
                }
                else
                {
                    rateLimitExceeded = false;
                    requestInfo.RequestTimes.Add(now);
                }
            }

            if (rateLimitExceeded)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = rateLimitAttribute.TimeWindow.TotalSeconds.ToString();

                _logger.LogWarning($"Rate limit exceeded for client {clientId}. " +
                                 $"Endpoint: {context.Request.Path}");

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many requests. Please try again after {rateLimitAttribute.TimeWindow.TotalSeconds} seconds.",
                    retryAfter = rateLimitAttribute.TimeWindow.TotalSeconds
                });

                return;
            }
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        // Fallback to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return $"ip_{ipAddress}";
        }

        // Last resort: session ID or random
        return $"session_{context.Session?.Id ?? Guid.NewGuid().ToString()}";
    }
}

public class ClientRequestInfo
{
    public List<DateTime> RequestTimes { get; set; } = new();
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : Attribute
{
    public int MaxRequests { get; set; }
    public TimeSpan TimeWindow { get; set; }

    public RateLimitAttribute(int maxRequests, int windowSeconds)
    {
        MaxRequests = maxRequests;
        TimeWindow = TimeSpan.FromSeconds(windowSeconds);
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
