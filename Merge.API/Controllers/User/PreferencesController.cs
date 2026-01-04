using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.DTOs.User;
using Merge.API.Middleware;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerine erişebilir
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/user/preferences")]
[Authorize]
public class PreferencesController : BaseController
{
    private readonly IUserPreferenceService _preferenceService;

    public PreferencesController(IUserPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerine erişebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> GetMyPreferences(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preferences = await _preferenceService.GetUserPreferencesAsync(userId, cancellationToken);
        return Ok(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerini güncelleyebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> UpdateMyPreferences(
        [FromBody] UpdateUserPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preferences = await _preferenceService.UpdateUserPreferencesAsync(userId, dto, cancellationToken);
        return Ok(preferences);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi tercihlerini sıfırlayabilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("reset")]
    [RateLimit(10, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 10/saat (reset işlemi sınırlı)
    [ProducesResponseType(typeof(UserPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserPreferenceDto>> ResetToDefaults(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preferences = await _preferenceService.ResetToDefaultsAsync(userId, cancellationToken);
        return Ok(preferences);
    }
}

