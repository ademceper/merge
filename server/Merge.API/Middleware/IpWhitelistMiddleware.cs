using System.Net;

namespace Merge.API.Middleware;

public class IpWhitelistMiddleware(RequestDelegate next, ILogger<IpWhitelistMiddleware> logger, IConfiguration configuration)
{
    private readonly List<string> allowedIPs = configuration.GetSection("IpWhitelist:AllowedIPs").Get<List<string>>() ?? new List<string>();

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
                logger.LogWarning("Forbidden request from IP {RemoteIp}. Endpoint: {Endpoint}",
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

        await next(context);
    }

    private bool IsIpAllowed(IPAddress ipAddress)
    {
        if (allowedIPs.Count == 0)
        {
            return true;
        }

        var ipString = ipAddress.ToString();

        if (allowedIPs.Contains(ipString))
        {
            return true;
        }

        if (IPAddress.IsLoopback(ipAddress))
        {
            return true;
        }
        foreach (var allowedIp in allowedIPs)
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
