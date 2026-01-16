using System.Net;
using System.Text.Json;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Merge.Application.Exceptions;
using Merge.Domain.Exceptions;

namespace Merge.API.Middleware;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior exception handling (ZORUNLU)
// ✅ BOLUM 4.1.4: RFC 7807 Problem Details (ZORUNLU)
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails();
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        switch (exception)
        {
            // ✅ BOLUM 2.1: FluentValidation ValidationException handling
            case FluentValidation.ValidationException validationEx:
                problemDetails.Type = "https://api.merge.com/errors/validation-error";
                problemDetails.Title = "Validation Error";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = "One or more validation errors occurred.";
                
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
                
                problemDetails.Extensions["errors"] = errors;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            
            case ArgumentNullException:
            case ArgumentException:
                problemDetails.Type = "https://api.merge.com/errors/bad-request";
                problemDetails.Title = "Bad Request";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
                
            case UnauthorizedAccessException:
                problemDetails.Type = "https://api.merge.com/errors/unauthorized";
                problemDetails.Title = "Unauthorized";
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
                
            case DomainException domainEx:
                problemDetails.Type = "https://api.merge.com/errors/domain-error";
                problemDetails.Title = "Domain Error";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = domainEx.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
                
            case NotFoundException:
            case KeyNotFoundException:
            case InvalidOperationException when exception.Message.Contains("not found") || exception.Message.Contains("bulunamadı"):
                problemDetails.Type = "https://api.merge.com/errors/not-found";
                problemDetails.Title = "Not Found";
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
                
            case BusinessException:
            case InvalidOperationException:
                problemDetails.Type = "https://api.merge.com/errors/business-error";
                problemDetails.Title = "Business Error";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            // ✅ ERROR HANDLING FIX: DbUpdateConcurrencyException - optimistic concurrency conflict
            case DbUpdateConcurrencyException:
                problemDetails.Type = "https://api.merge.com/errors/concurrency-conflict";
                problemDetails.Title = "Concurrency Conflict";
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = "The record has been modified by another user. Please refresh and try again.";
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                break;

            // ✅ ERROR HANDLING FIX: DbUpdateException - database constraint violations
            case DbUpdateException dbEx:
                problemDetails.Type = "https://api.merge.com/errors/database-error";
                problemDetails.Title = "Database Error";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Instance = context.Request.Path;
                // User-friendly message, hide internal details
                problemDetails.Detail = GetDbErrorMessage(dbEx);
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            default:
                problemDetails.Type = "https://api.merge.com/errors/internal-server-error";
                problemDetails.Title = "An unexpected error occurred";
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = _environment.IsDevelopment() ? exception.Message : null;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                // ✅ SECURITY FIX: Stack trace sadece Development'ta ve sınırlı bilgi ile
                if (_environment.IsDevelopment())
                {
                    problemDetails.Extensions["exception"] = exception.GetType().Name;
                    // Stack trace'i sadece ilk 500 karakter ile sınırla (güvenlik için)
                    var stackTrace = exception.StackTrace;
                    if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Length > 500)
                    {
                        stackTrace = stackTrace.Substring(0, 500) + "... (truncated)";
                    }
                    problemDetails.Extensions["stackTrace"] = stackTrace;
                }
                
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.ContentType = "application/problem+json";
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };
        
        var result = JsonSerializer.Serialize(problemDetails, options);
        return context.Response.WriteAsync(result);
    }

    // ✅ ERROR HANDLING FIX: User-friendly database error messages
    private static string GetDbErrorMessage(DbUpdateException ex)
    {
        var innerMessage = ex.InnerException?.Message ?? ex.Message;

        // PostgreSQL error codes
        if (innerMessage.Contains("23505") || innerMessage.Contains("duplicate key"))
        {
            return "A record with this identifier already exists.";
        }

        if (innerMessage.Contains("23503") || innerMessage.Contains("foreign key"))
        {
            return "The operation cannot be completed because it references data that no longer exists.";
        }

        if (innerMessage.Contains("23502") || innerMessage.Contains("not-null"))
        {
            return "A required field is missing.";
        }

        if (innerMessage.Contains("23514") || innerMessage.Contains("check constraint"))
        {
            return "The data provided violates a business rule.";
        }

        // Generic fallback - don't expose internal details
        return "A database error occurred. Please try again.";
    }
}

