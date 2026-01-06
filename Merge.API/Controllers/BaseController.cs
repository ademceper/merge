using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Merge.API.Controllers;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseController : ControllerBase
{
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

    /// <summary>
    /// ModelState validation kontrolü yapar. Geçersizse BadRequest döner.
    /// </summary>
    protected ActionResult? ValidateModelState()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return null; // ModelState geçerli, null döner (çağıran kod devam edebilir)
    }
}

