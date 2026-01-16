using System.Net;

namespace Merge.API.Middleware;

public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly HashSet<string> _allowedIPs;

    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        var allowedIPs = configuration.GetSection("Security:AllowedIPs").Get<string[]>() ?? Array.Empty<string>();
        _allowedIPs = new HashSet<string>(allowedIPs);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var ipWhitelistAttribute = endpoint?.Metadata.GetMetadata<IpWhitelistAttribute>();

        if (ipWhitelistAttribute != null)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            if (remoteIp == null || !IsIpAllowed(remoteIp))
            {
                // ✅ LOGGING FIX: Structured logging kullan (string interpolation yerine)
                _logger.LogWarning("Forbidden request from IP {RemoteIp}. Endpoint: {Endpoint}",
                    remoteIp, context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = "Access denied from this IP address"
                });

                return;
            }
        }

        await _next(context);
    }

    private bool IsIpAllowed(IPAddress ipAddress)
    {
        if (_allowedIPs.Count == 0)
        {
            return true;
        }

        var ipString = ipAddress.ToString();

        if (_allowedIPs.Contains(ipString))
        {
            return true;
        }

        if (IPAddress.IsLoopback(ipAddress))
        {
            return true;
        }
        foreach (var allowedIp in _allowedIPs)
        {
            if (allowedIp.Contains("/"))
            {
                // CIDR notation support can be added here
                // For now, just exact matches
            }
        }

        return false;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class IpWhitelistAttribute : Attribute
{
}

public static class IpWhitelistMiddlewareExtensions
{
    public static IApplicationBuilder UseIpWhitelist(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpWhitelistMiddleware>();
    }
}
