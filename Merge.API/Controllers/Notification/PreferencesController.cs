using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;


namespace Merge.API.Controllers.Notification;

[ApiController]
[Route("api/notifications/preferences")]
[Authorize]
public class NotificationPreferencesController : BaseController
{
    private readonly INotificationPreferenceService _notificationPreferenceService;

    public NotificationPreferencesController(INotificationPreferenceService notificationPreferenceService)
    {
        _notificationPreferenceService = notificationPreferenceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationPreferenceDto>>> GetMyPreferences()
    {
        var userId = GetUserId();
        var preferences = await _notificationPreferenceService.GetUserPreferencesAsync(userId);
        return Ok(preferences);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<NotificationPreferenceSummaryDto>> GetMyPreferencesSummary()
    {
        var userId = GetUserId();
        var summary = await _notificationPreferenceService.GetUserPreferencesSummaryAsync(userId);
        return Ok(summary);
    }

    [HttpGet("{notificationType}/{channel}")]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreference(string notificationType, string channel)
    {
        var userId = GetUserId();
        var preference = await _notificationPreferenceService.GetPreferenceAsync(userId, notificationType, channel);
        if (preference == null)
        {
            return NotFound();
        }
        return Ok(preference);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationPreferenceDto>> CreatePreference([FromBody] CreateNotificationPreferenceDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var preference = await _notificationPreferenceService.CreatePreferenceAsync(userId, dto);
        return CreatedAtAction(nameof(GetPreference), new { notificationType = dto.NotificationType, channel = dto.Channel }, preference);
    }

    [HttpPut("{notificationType}/{channel}")]
    public async Task<ActionResult<NotificationPreferenceDto>> UpdatePreference(
        string notificationType, 
        string channel, 
        [FromBody] UpdateNotificationPreferenceDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var preference = await _notificationPreferenceService.UpdatePreferenceAsync(userId, notificationType, channel, dto);
        if (preference == null)
        {
            return NotFound();
        }
        return Ok(preference);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkUpdatePreferences([FromBody] BulkUpdateNotificationPreferencesDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var success = await _notificationPreferenceService.BulkUpdatePreferencesAsync(userId, dto);
        return NoContent();
    }

    [HttpDelete("{notificationType}/{channel}")]
    public async Task<IActionResult> DeletePreference(string notificationType, string channel)
    {
        var userId = GetUserId();
        var success = await _notificationPreferenceService.DeletePreferenceAsync(userId, notificationType, channel);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("check/{notificationType}/{channel}")]
    public async Task<ActionResult<bool>> CheckNotificationEnabled(string notificationType, string channel)
    {
        var userId = GetUserId();
        var isEnabled = await _notificationPreferenceService.IsNotificationEnabledAsync(userId, notificationType, channel);
        return Ok(new { isEnabled });
    }

    [HttpGet("channels/{notificationType}")]
    public async Task<ActionResult<IEnumerable<string>>> GetEnabledChannels(string notificationType)
    {
        var userId = GetUserId();

        var channels = await _notificationPreferenceService.GetEnabledChannelsAsync(userId, notificationType);
        return Ok(channels);
    }
}

