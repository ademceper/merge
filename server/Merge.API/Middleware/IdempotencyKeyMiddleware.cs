using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Merge.API.Middleware;

/// <summary>
/// Idempotency Key Middleware
/// HIGH-API-002: Idempotency Keys - Prevents duplicate processing of POST/PUT requests
/// </summary>
public class IdempotencyKeyMiddleware(RequestDelegate next, ILogger<IdempotencyKeyMiddleware> logger, IDistributedCache cache)
{

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process POST, PUT, PATCH requests
        if (!IsIdempotentMethod(context.Request.Method))
        {
            await next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers["X-Idempotency-Key"].FirstOrDefault();

        // If no idempotency key provided, continue (optional for now)
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await next(context);
            return;
        }

        // Validate idempotency key format (should be GUID or UUID)
        if (!Guid.TryParse(idempotencyKey, out _))
        {
            logger.LogWarning("Invalid idempotency key format. Key: {Key}", idempotencyKey);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://api.merge.com/errors/invalid-idempotency-key",
                title = "Invalid Idempotency Key",
                status = StatusCodes.Status400BadRequest,
                detail = "X-Idempotency-Key must be a valid GUID/UUID format."
            }), Encoding.UTF8);
            return;
        }

        // Check if this request was already processed
        var cacheKey = $"idempotency:{idempotencyKey}";
        var cachedResponse = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResponse))
        {
            logger.LogInformation("Idempotent request detected. Key: {Key}", idempotencyKey);
            
            // Return cached response
            var cachedData = JsonSerializer.Deserialize<IdempotencyResponse>(cachedResponse);
            if (cachedData != null)
            {
                context.Response.StatusCode = cachedData.StatusCode;
                context.Response.ContentType = cachedData.ContentType;
                
                // Copy headers from cached response
                foreach (var header in cachedData.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }
                
                await context.Response.WriteAsync(cachedData.Body, Encoding.UTF8);
                return;
            }
        }

        // Enable response buffering to capture response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);

            // Capture response
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Cache successful responses (2xx status codes)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                var responseData = new IdempotencyResponse
                {
                    StatusCode = context.Response.StatusCode,
                    ContentType = context.Response.ContentType ?? "application/json",
                    Body = responseBodyText,
                    Headers = context.Response.Headers.ToDictionary(
                        h => h.Key,
                        h => h.Value.ToString())
                };

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Cache for 24 hours
                };

                await cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(responseData),
                    cacheOptions);
            }

            // Copy response to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static bool IsIdempotentMethod(string method)
    {
        return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
    }

    private class IdempotencyResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
    }
}
