using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Security;
using Merge.Application.DTOs.Security;

namespace Merge.API.Controllers.Security;

[ApiController]
[Route("api/security/account-monitoring")]
[Authorize]
public class AccountSecurityMonitoringsController : BaseController
{
    private readonly IAccountSecurityMonitoringService _accountSecurityMonitoringService;
        public AccountSecurityMonitoringsController(
        IAccountSecurityMonitoringService accountSecurityMonitoringService)
    {
        _accountSecurityMonitoringService = accountSecurityMonitoringService;
            }

    [HttpPost("events")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AccountSecurityEventDto>> LogSecurityEvent([FromBody] CreateAccountSecurityEventDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (string.IsNullOrEmpty(dto.IpAddress))
        {
            dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        if (string.IsNullOrEmpty(dto.UserAgent))
        {
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
        }

        var securityEvent = await _accountSecurityMonitoringService.LogSecurityEventAsync(dto);
        return CreatedAtAction(nameof(GetUserSecurityEvents), new { userId = securityEvent.UserId }, securityEvent);
    }

    [HttpGet("events/user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AccountSecurityEventDto>>> GetUserSecurityEvents(
        Guid userId,
        [FromQuery] string? eventType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var events = await _accountSecurityMonitoringService.GetUserSecurityEventsAsync(userId, eventType, page, pageSize);
        return Ok(events);
    }

    [HttpGet("events/suspicious")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AccountSecurityEventDto>>> GetSuspiciousEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var events = await _accountSecurityMonitoringService.GetSuspiciousEventsAsync(page, pageSize);
        return Ok(events);
    }

    [HttpPost("events/{eventId}/action")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> TakeAction(Guid eventId, [FromBody] TakeActionDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var actionTakenByUserId = GetUserId();
        var result = await _accountSecurityMonitoringService.TakeActionAsync(eventId, actionTakenByUserId, dto.Action, dto.Notes);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SecurityAlertDto>> CreateSecurityAlert([FromBody] CreateSecurityAlertDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var alert = await _accountSecurityMonitoringService.CreateSecurityAlertAsync(dto);
        return CreatedAtAction(nameof(GetSecurityAlerts), new { userId = alert.UserId }, alert);
    }

    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<SecurityAlertDto>>> GetSecurityAlerts(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var alerts = await _accountSecurityMonitoringService.GetSecurityAlertsAsync(userId, severity, status, page, pageSize);
        return Ok(alerts);
    }

    [HttpPost("alerts/{alertId}/acknowledge")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AcknowledgeAlert(Guid alertId)
    {
        var acknowledgedByUserId = GetUserId();
        var result = await _accountSecurityMonitoringService.AcknowledgeAlertAsync(alertId, acknowledgedByUserId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("alerts/{alertId}/resolve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ResolveAlert(Guid alertId, [FromBody] ResolveAlertDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var resolvedByUserId = GetUserId();
        var result = await _accountSecurityMonitoringService.ResolveAlertAsync(alertId, resolvedByUserId, dto.ResolutionNotes);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SecurityMonitoringSummaryDto>> GetSecuritySummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var summary = await _accountSecurityMonitoringService.GetSecuritySummaryAsync(startDate, endDate);
        return Ok(summary);
    }
}

