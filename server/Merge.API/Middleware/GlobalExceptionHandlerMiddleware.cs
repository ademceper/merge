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

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails();
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        switch (exception)
        {
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

            case Merge.Application.Exceptions.ValidationException validationEx:
                problemDetails.Type = $"https://api.merge.com/errors/{validationEx.ErrorCode.ToLowerInvariant().Replace("_", "-")}";
                problemDetails.Title = "Validation Error";
                problemDetails.Status = validationEx.HttpStatusCode; // 400
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = validationEx.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                problemDetails.Extensions["errorCode"] = validationEx.ErrorCode;

                // Validation errors ekle
                if (validationEx.Errors.Count > 0)
                {
                    problemDetails.Extensions["errors"] = validationEx.Errors;
                }

                // Metadata ekle
                foreach (var (key, value) in validationEx.Metadata)
                {
                    problemDetails.Extensions[key] = value;
                }

                context.Response.StatusCode = validationEx.HttpStatusCode;
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
                problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity; // 422 - Business rule violation
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = domainEx.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                break;
                
            case NotFoundException notFoundEx:
                problemDetails.Type = $"https://api.merge.com/errors/{notFoundEx.ErrorCode.ToLowerInvariant().Replace("_", "-")}";
                problemDetails.Title = "Not Found";
                problemDetails.Status = notFoundEx.HttpStatusCode; // 404
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = notFoundEx.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                problemDetails.Extensions["errorCode"] = notFoundEx.ErrorCode;

                // Metadata ekle
                foreach (var (key, value) in notFoundEx.Metadata)
                {
                    problemDetails.Extensions[key] = value;
                }

                context.Response.StatusCode = notFoundEx.HttpStatusCode;
                break;

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
                
            case BusinessException businessEx:
                problemDetails.Type = $"https://api.merge.com/errors/{businessEx.ErrorCode.ToLowerInvariant().Replace("_", "-")}";
                problemDetails.Title = "Business Error";
                problemDetails.Status = businessEx.HttpStatusCode; // 422 - Unprocessable Entity
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = businessEx.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                problemDetails.Extensions["errorCode"] = businessEx.ErrorCode;

                // Metadata ekle
                foreach (var (key, value) in businessEx.Metadata)
                {
                    problemDetails.Extensions[key] = value;
                }

                context.Response.StatusCode = businessEx.HttpStatusCode;
                break;

            case InvalidOperationException:
                problemDetails.Type = "https://api.merge.com/errors/invalid-operation";
                problemDetails.Title = "Invalid Operation";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

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

            case ConfigurationException configEx:
                problemDetails.Type = $"https://api.merge.com/errors/{configEx.ErrorCode.ToLowerInvariant().Replace("_", "-")}";
                problemDetails.Title = "Configuration Error";
                problemDetails.Status = configEx.HttpStatusCode; // 500
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = configEx.Message;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                problemDetails.Extensions["errorCode"] = configEx.ErrorCode;

                // Metadata ekle
                foreach (var (key, value) in configEx.Metadata)
                {
                    problemDetails.Extensions[key] = value;
                }

                context.Response.StatusCode = configEx.HttpStatusCode;
                break;

            default:
                problemDetails.Type = "https://api.merge.com/errors/internal-server-error";
                problemDetails.Title = "An unexpected error occurred";
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Detail = environment.IsDevelopment() ? exception.Message : null;
                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
                
                if (environment.IsDevelopment())
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
            WriteIndented = environment.IsDevelopment()
        };
        
        var result = JsonSerializer.Serialize(problemDetails, options);
        return context.Response.WriteAsync(result);
    }

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

