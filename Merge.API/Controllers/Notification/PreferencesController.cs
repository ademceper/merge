using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Notification;
using Merge.Application.DTOs.Notification;
using Merge.Application.Common;
using Merge.API.Middleware;

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

    /// <summary>
    /// Kullanıcının bildirim tercihlerini getirir (pagination ile)
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<NotificationPreferenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<NotificationPreferenceDto>>> GetMyPreferences(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var allPreferences = await _notificationPreferenceService.GetUserPreferencesAsync(userId, cancellationToken);
        var preferencesList = allPreferences.ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = preferencesList.Count;
        var pagedPreferences = preferencesList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<NotificationPreferenceDto>
        {
            Items = pagedPreferences,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının bildirim tercih özetini getirir
    /// </summary>
    [HttpGet("summary")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(NotificationPreferenceSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceSummaryDto>> GetMyPreferencesSummary(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var summary = await _notificationPreferenceService.GetUserPreferencesSummaryAsync(userId, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Belirli bir bildirim tercihini getirir
    /// </summary>
    [HttpGet("{notificationType}/{channel}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(NotificationPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreference(
        string notificationType,
        string channel,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var preference = await _notificationPreferenceService.GetPreferenceAsync(userId, notificationType, channel, cancellationToken);
        if (preference == null)
        {
            return NotFound();
        }
        return Ok(preference);
    }

    /// <summary>
    /// Yeni bildirim tercihi oluşturur
    /// </summary>
    [HttpPost]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(NotificationPreferenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceDto>> CreatePreference(
        [FromBody] CreateNotificationPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var preference = await _notificationPreferenceService.CreatePreferenceAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetPreference), new { notificationType = dto.NotificationType, channel = dto.Channel }, preference);
    }

    /// <summary>
    /// Bildirim tercihini günceller
    /// </summary>
    [HttpPut("{notificationType}/{channel}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(NotificationPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceDto>> UpdatePreference(
        string notificationType, 
        string channel, 
        [FromBody] UpdateNotificationPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var preference = await _notificationPreferenceService.UpdatePreferenceAsync(userId, notificationType, channel, dto, cancellationToken);
        if (preference == null)
        {
            return NotFound();
        }
        return Ok(preference);
    }

    /// <summary>
    /// Toplu bildirim tercihi günceller
    /// </summary>
    [HttpPost("bulk")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BulkUpdatePreferences(
        [FromBody] BulkUpdateNotificationPreferencesDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _notificationPreferenceService.BulkUpdatePreferencesAsync(userId, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Bildirim tercihini siler
    /// </summary>
    [HttpDelete("{notificationType}/{channel}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePreference(
        string notificationType,
        string channel,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _notificationPreferenceService.DeletePreferenceAsync(userId, notificationType, channel, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Bildirimin etkin olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("check/{notificationType}/{channel}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> CheckNotificationEnabled(
        string notificationType,
        string channel,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isEnabled = await _notificationPreferenceService.IsNotificationEnabledAsync(userId, notificationType, channel, cancellationToken);
        return Ok(new { isEnabled });
    }

    /// <summary>
    /// Belirli bir bildirim tipi için etkin kanalları getirir
    /// </summary>
    [HttpGet("channels/{notificationType}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<string>>> GetEnabledChannels(
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var channels = await _notificationPreferenceService.GetEnabledChannelsAsync(userId, notificationType, cancellationToken);
        return Ok(channels);
    }
}
