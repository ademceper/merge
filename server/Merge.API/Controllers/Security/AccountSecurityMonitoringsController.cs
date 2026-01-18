using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Security.Commands.LogSecurityEvent;
using Merge.Application.Security.Commands.CreateSecurityAlert;
using Merge.Application.Security.Commands.AcknowledgeAlert;
using Merge.Application.Security.Commands.ResolveAlert;
using Merge.Application.Security.Commands.TakeAction;
using Merge.Application.Security.Queries.GetUserSecurityEvents;
using Merge.Application.Security.Queries.GetSuspiciousEvents;
using Merge.Application.Security.Queries.GetSecurityAlerts;
using Merge.Application.Security.Queries.GetSecuritySummary;

namespace Merge.API.Controllers.Security;

/// <summary>
/// Account Security Monitoring API endpoints.
/// Hesap güvenlik izleme işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/security/account-monitoring")]
[Authorize]
[Tags("AccountSecurityMonitoring")]
public class AccountSecurityMonitoringsController(IMediator mediator, IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpPost("events")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(AccountSecurityEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AccountSecurityEventDto>> LogSecurityEvent(
        [FromBody] CreateAccountSecurityEventDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(dto.IpAddress))
        {
            dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        if (string.IsNullOrEmpty(dto.UserAgent))
        {
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
        }
        var command = new LogSecurityEventCommand(
            dto.UserId,
            dto.EventType,
            dto.Severity,
            dto.IpAddress,
            dto.UserAgent,
            dto.Location,
            dto.DeviceFingerprint,
            dto.IsSuspicious,
            dto.Details,
            dto.RequiresAction);
        var securityEvent = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUserSecurityEvents), new { userId = securityEvent.UserId }, securityEvent);
    }

    [HttpGet("events/user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<AccountSecurityEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<AccountSecurityEventDto>>> GetUserSecurityEvents(
        Guid userId,
        [FromQuery] string? eventType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetUserSecurityEventsQuery(userId, eventType, page, pageSize);
        var events = await mediator.Send(query, cancellationToken);
        return Ok(events);
    }

    [HttpGet("events/suspicious")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<AccountSecurityEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<AccountSecurityEventDto>>> GetSuspiciousEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetSuspiciousEventsQuery(page, pageSize);
        var events = await mediator.Send(query, cancellationToken);
        return Ok(events);
    }

    [HttpPost("events/{eventId}/action")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TakeAction(
        Guid eventId,
        [FromBody] TakeActionDto dto,
        CancellationToken cancellationToken = default)
    {
        var actionTakenByUserId = GetUserId();
        var command = new TakeActionCommand(eventId, actionTakenByUserId, dto.Action, dto.Notes);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SecurityAlertDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SecurityAlertDto>> CreateSecurityAlert(
        [FromBody] CreateSecurityAlertDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSecurityAlertCommand(
            dto.UserId,
            dto.AlertType,
            dto.Severity,
            dto.Title,
            dto.Description,
            dto.Metadata);
        var alert = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSecurityAlerts), new { userId = alert.UserId }, alert);
    }

    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<SecurityAlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SecurityAlertDto>>> GetSecurityAlerts(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetSecurityAlertsQuery(userId, severity, status, page, pageSize);
        var alerts = await mediator.Send(query, cancellationToken);
        return Ok(alerts);
    }

    [HttpPost("alerts/{alertId}/acknowledge")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AcknowledgeAlert(
        Guid alertId,
        CancellationToken cancellationToken = default)
    {
        var acknowledgedByUserId = GetUserId();
        var command = new AcknowledgeAlertCommand(alertId, acknowledgedByUserId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("alerts/{alertId}/resolve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResolveAlert(
        Guid alertId,
        [FromBody] ResolveAlertDto dto,
        CancellationToken cancellationToken = default)
    {
        var resolvedByUserId = GetUserId();
        var command = new ResolveAlertCommand(alertId, resolvedByUserId, dto.ResolutionNotes);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SecurityMonitoringSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SecurityMonitoringSummaryDto>> GetSecuritySummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSecuritySummaryQuery(startDate, endDate);
        var summary = await mediator.Send(query, cancellationToken);
        return Ok(summary);
    }
}
