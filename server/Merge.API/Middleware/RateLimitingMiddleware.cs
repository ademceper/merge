using System.Collections.Concurrent;
using System.Net;

namespace Merge.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clientRequests = new();

    // ✅ MEMORY LEAK FIX: Cleanup interval and last cleanup time
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan StaleEntryThreshold = TimeSpan.FromMinutes(10);
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly object _cleanupLock = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ✅ MEMORY LEAK FIX: Periyodik olarak stale entry'leri temizle
        CleanupStaleEntriesIfNeeded();

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

                // Update last access time for cleanup tracking
                requestInfo.LastAccessTime = now;

                requestInfo.RequestTimes.RemoveAll(t => now - t > rateLimitAttribute.TimeWindow);

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

                // ✅ LOGGING FIX: Structured logging kullan (string interpolation yerine)
                _logger.LogWarning("Rate limit exceeded for client {ClientId}. Endpoint: {Endpoint}",
                    clientId, context.Request.Path);

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

    // ✅ MEMORY LEAK FIX: Cleanup stale entries periodically
    private void CleanupStaleEntriesIfNeeded()
    {
        var now = DateTime.UtcNow;

        // Quick check without lock
        if (now - _lastCleanup < CleanupInterval)
        {
            return;
        }

        // Double-check with lock to prevent concurrent cleanup
        lock (_cleanupLock)
        {
            if (now - _lastCleanup < CleanupInterval)
            {
                return;
            }

            _lastCleanup = now;

            var staleKeys = _clientRequests
                .Where(kvp => now - kvp.Value.LastAccessTime > StaleEntryThreshold)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in staleKeys)
            {
                _clientRequests.TryRemove(key, out _);
            }

            if (staleKeys.Count > 0)
            {
                _logger.LogDebug("Rate limiter cleanup: Removed {Count} stale entries. Current entries: {Total}",
                    staleKeys.Count, _clientRequests.Count);
            }
        }
    }

    private string GetClientIdentifier(HttpContext context)
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return $"ip_{ipAddress}";
        }

        return $"session_{context.Session?.Id ?? Guid.NewGuid().ToString()}";
    }
}

// ✅ MEMORY LEAK FIX: LastAccessTime added for cleanup tracking
public class ClientRequestInfo
{
    public List<DateTime> RequestTimes { get; set; } = [];
    public DateTime LastAccessTime { get; set; } = DateTime.UtcNow;
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
