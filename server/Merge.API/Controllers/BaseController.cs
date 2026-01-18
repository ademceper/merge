using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Diagnostics;
using MediatR;

namespace Merge.API.Controllers;

/// <summary>
/// Tüm API controller'ları için base class.
/// Ortak davranışları ve helper method'ları içerir.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class BaseController : ControllerBase
{
    
    protected IMediator Mediator =>
        HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Current user ID.
    /// </summary>
    protected Guid? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } id
            ? Guid.Parse(id)
            : null;

    /// <summary>
    /// Current user roles.
    /// </summary>
    protected IEnumerable<string> CurrentUserRoles =>
        User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    /// <summary>
    /// Created response with location header.
    /// </summary>
    protected CreatedAtActionResult CreatedAtAction<T>(
        string actionName,
        object routeValues,
        T value)
    {
        return base.CreatedAtAction(actionName, routeValues, value);
    }

    /// <summary>
    /// Problem details response.
    /// </summary>
    protected ObjectResult Problem(
        string detail,
        string title,
        int statusCode,
        string? type = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type ?? GetProblemType(statusCode),
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return StatusCode(statusCode, problemDetails);
    }

    private static string GetProblemType(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        429 => "https://tools.ietf.org/html/rfc6585#section-4",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    // Backward compatibility methods
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");
        }
        return userId;
    }

    protected Guid? GetUserIdOrNull()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }

    protected bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
        {
            return false;
        }
        return true;
    }

    protected bool TryGetUserRole(out string role)
    {
        role = string.Empty;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            return false;
        }
        role = roleClaim;
        return true;
    }

    protected ActionResult? ValidateModelState()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return null;
    }
}

