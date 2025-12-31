using System.Net;
using System.Text.Json;

namespace Merge.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "Bir hata oluştu.";

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;
            case KeyNotFoundException:
            case InvalidOperationException when exception.Message.Contains("not found") || exception.Message.Contains("bulunamadı"):
                code = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            default:
                if (exception.Message.Contains("bulunamadı") || exception.Message.Contains("not found"))
                {
                    code = HttpStatusCode.NotFound;
                    message = exception.Message;
                }
                else if (exception.Message.Contains("yetersiz") || exception.Message.Contains("insufficient") || 
                         exception.Message.Contains("geçersiz") || exception.Message.Contains("invalid"))
                {
                    code = HttpStatusCode.BadRequest;
                    message = exception.Message;
                }
                else
                {
                    message = exception.Message;
                }
                break;
        }

        var result = JsonSerializer.Serialize(new { message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

