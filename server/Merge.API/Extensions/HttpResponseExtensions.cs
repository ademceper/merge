using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Merge.API.Extensions;

/// <summary>
/// HTTP Response Extensions for ETag and Cache-Control headers
/// HIGH-API-003: ETag/Cache-Control Headers - HTTP caching support for GET endpoints
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Sets ETag header based on response content hash
    /// </summary>
    public static void SetETag(this HttpResponse response, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        var hash = ComputeHash(content);
        var etag = $"\"{hash}\"";
        response.Headers[HeaderNames.ETag] = etag;
    }

    /// <summary>
    /// Sets ETag header from entity's version/timestamp
    /// </summary>
    public static void SetETag(this HttpResponse response, byte[]? rowVersion)
    {
        if (rowVersion == null || rowVersion.Length == 0)
        {
            return;
        }

        var hash = Convert.ToBase64String(rowVersion);
        var etag = $"\"{hash}\"";
        response.Headers[HeaderNames.ETag] = etag;
    }

    /// <summary>
    /// Sets ETag header from entity's last modified timestamp
    /// </summary>
    public static void SetETag(this HttpResponse response, DateTime lastModified)
    {
        var hash = lastModified.Ticks.GetHashCode().ToString("X");
        var etag = $"\"{hash}\"";
        response.Headers[HeaderNames.ETag] = etag;
    }

    /// <summary>
    /// Sets Cache-Control header with specified max-age
    /// </summary>
    public static void SetCacheControl(this HttpResponse response, int maxAgeSeconds, bool isPublic = false)
    {
        var cacheControl = isPublic
            ? $"public, max-age={maxAgeSeconds}"
            : $"private, max-age={maxAgeSeconds}";
        response.Headers[HeaderNames.CacheControl] = cacheControl;
    }

    /// <summary>
    /// Sets Cache-Control header to no-cache
    /// </summary>
    public static void SetNoCache(this HttpResponse response)
    {
        response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "0";
    }

    /// <summary>
    /// Checks if request has matching ETag (304 Not Modified)
    /// </summary>
    public static bool IsNotModified(this HttpRequest request, string etag)
    {
        var ifNoneMatch = request.Headers[HeaderNames.IfNoneMatch].FirstOrDefault();
        return !string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etag;
    }

    /// <summary>
    /// Checks if request has matching ETag (304 Not Modified)
    /// </summary>
    public static bool IsNotModified(this HttpRequest request, byte[]? rowVersion)
    {
        if (rowVersion == null || rowVersion.Length == 0)
        {
            return false;
        }

        var hash = Convert.ToBase64String(rowVersion);
        var etag = $"\"{hash}\"";
        return request.IsNotModified(etag);
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)[..16]; // Use first 16 chars for shorter ETag
    }
}
