using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.DTOs.User;


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

    [HttpGet]
    public async Task<ActionResult<UserPreferenceDto>> GetMyPreferences()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preferences = await _preferenceService.GetUserPreferencesAsync(userId);
        return Ok(preferences);
    }

    [HttpPut]
    public async Task<ActionResult<UserPreferenceDto>> UpdateMyPreferences([FromBody] UpdateUserPreferenceDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preferences = await _preferenceService.UpdateUserPreferencesAsync(userId, dto);
        return Ok(preferences);
    }

    [HttpPost("reset")]
    public async Task<ActionResult<UserPreferenceDto>> ResetToDefaults()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var preferences = await _preferenceService.ResetToDefaultsAsync(userId);
        return Ok(preferences);
    }
}

