using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Notification;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Notification.Commands.BulkUpdatePreferences;
using Merge.Application.Notification.Commands.CreatePreference;
using Merge.Application.Notification.Commands.DeletePreference;
using Merge.Application.Notification.Commands.UpdatePreference;
using Merge.Application.Notification.Queries.GetEnabledChannels;
using Merge.Application.Notification.Queries.GetPreference;
using Merge.Application.Notification.Queries.GetUserPreferences;
using Merge.Application.Notification.Queries.GetUserPreferencesSummary;
using Merge.Application.Notification.Queries.IsNotificationEnabled;
using Merge.API.Middleware;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Notification;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/notifications/preferences")]
[Authorize]
public class NotificationPreferencesController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<NotificationPreferenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<NotificationPreferenceDto>>> GetMyPreferences(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;
        var userId = GetUserId();
        var query = new GetUserPreferencesQuery(userId, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(NotificationPreferenceSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceSummaryDto>> GetMyPreferencesSummary(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserPreferencesSummaryQuery(userId);
        var summary = await mediator.Send(query, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("{notificationType}/{channel}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(NotificationPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreference(
        string notificationType,
        string channel,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<NotificationChannel>(channel, true, out var channelEnum))
        {
            return BadRequest("Geçersiz notification type veya channel.");
        }

        var userId = GetUserId();
        var query = new GetPreferenceQuery(userId, notificationTypeEnum, channelEnum);
        var preference = await mediator.Send(query, cancellationToken);
        if (preference == null)
        {
            return NotFound();
        }
        return Ok(preference);
    }

    [HttpPost]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(NotificationPreferenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<NotificationPreferenceDto>> CreatePreference(
        [FromBody] CreateNotificationPreferenceDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new CreatePreferenceCommand(userId, dto);
        var preference = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPreference), new { notificationType = dto.NotificationType.ToString(), channel = dto.Channel.ToString() }, preference);
    }

    [HttpPut("{notificationType}/{channel}")]
    [RateLimit(30, 60)]
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
        if (!Enum.TryParse<NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<NotificationChannel>(channel, true, out var channelEnum))
        {
            return BadRequest("Geçersiz notification type veya channel.");
        }

        var userId = GetUserId();
        var command = new UpdatePreferenceCommand(userId, notificationTypeEnum, channelEnum, dto);
        var preference = await mediator.Send(command, cancellationToken);
        return Ok(preference);
    }

    [HttpPost("bulk")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BulkUpdatePreferences(
        [FromBody] BulkUpdateNotificationPreferencesDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new BulkUpdatePreferencesCommand(userId, dto);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{notificationType}/{channel}")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePreference(
        string notificationType,
        string channel,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<NotificationChannel>(channel, true, out var channelEnum))
        {
            return BadRequest("Geçersiz notification type veya channel.");
        }

        var userId = GetUserId();
        var command = new DeletePreferenceCommand(userId, notificationTypeEnum, channelEnum);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("check/{notificationType}/{channel}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> CheckNotificationEnabled(
        string notificationType,
        string channel,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationType>(notificationType, true, out var notificationTypeEnum) ||
            !Enum.TryParse<NotificationChannel>(channel, true, out var channelEnum))
        {
            return BadRequest("Geçersiz notification type veya channel.");
        }

        var userId = GetUserId();
        var query = new IsNotificationEnabledQuery(userId, notificationTypeEnum, channelEnum);
        var isEnabled = await mediator.Send(query, cancellationToken);
        return Ok(new { isEnabled });
    }

    [HttpGet("channels/{notificationType}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<string>>> GetEnabledChannels(
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationType>(notificationType, true, out var notificationTypeEnum))
        {
            return BadRequest("Geçersiz notification type.");
        }

        var userId = GetUserId();
        var query = new GetEnabledChannelsQuery(userId, notificationTypeEnum);
        var channels = await mediator.Send(query, cancellationToken);
        return Ok(channels);
    }
}
