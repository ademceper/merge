using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Governance;
using Merge.Application.DTOs.Security;


namespace Merge.API.Controllers.Governance;

[ApiController]
[Route("api/governance/audit-logs")]
[Authorize(Roles = "Admin,Manager")]
public class AuditLogsController : BaseController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Create an audit log entry
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogDto auditDto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = Request.Headers["User-Agent"].ToString();

        // Get user info from claims if not provided
        if (!auditDto.UserId.HasValue && User.Identity?.IsAuthenticated == true)
        {
            auditDto.UserId = GetUserIdOrNull();
        }

        if (string.IsNullOrEmpty(auditDto.UserEmail) && User.Identity?.IsAuthenticated == true)
        {
            auditDto.UserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        }

        await _auditLogService.LogAsync(auditDto, ipAddress, userAgent);
        return Ok();
    }

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AuditLogDto>> GetAuditById(Guid id)
    {
        var audit = await _auditLogService.GetAuditByIdAsync(id);

        if (audit == null)
        {
            return NotFound();
        }

        return Ok(audit);
    }

    /// <summary>
    /// Get audit logs with filtering
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> SearchAuditLogs(
        [FromBody] AuditLogFilterDto filter)
    {
        var audits = await _auditLogService.GetAuditLogsAsync(filter);
        return Ok(audits);
    }

    /// <summary>
    /// Get entity audit history
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<EntityAuditHistoryDto>> GetEntityHistory(
        string entityType,
        Guid entityId)
    {
        var history = await _auditLogService.GetEntityHistoryAsync(entityType, entityId);

        if (history == null)
        {
            return NotFound();
        }

        return Ok(history);
    }

    /// <summary>
    /// Get audit statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AuditStatsDto>> GetAuditStats([FromQuery] int days = 30)
    {
        var stats = await _auditLogService.GetAuditStatsAsync(days);
        return Ok(stats);
    }

    /// <summary>
    /// Get user audit history
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetUserAuditHistory(
        Guid userId,
        [FromQuery] int days = 30)
    {
        var audits = await _auditLogService.GetUserAuditHistoryAsync(userId, days);
        return Ok(audits);
    }

    /// <summary>
    /// Get my audit history
    /// </summary>
    [HttpGet("my-history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetMyAuditHistory([FromQuery] int days = 30)
    {
        var userId = GetUserId();
        var audits = await _auditLogService.GetUserAuditHistoryAsync(userId, days);
        return Ok(audits);
    }

    /// <summary>
    /// Compare old and new values in an audit log
    /// </summary>
    [HttpGet("{auditLogId}/compare")]
    public async Task<ActionResult<IEnumerable<AuditComparisonDto>>> CompareChanges(Guid auditLogId)
    {
        var comparisons = await _auditLogService.CompareChangesAsync(auditLogId);
        return Ok(comparisons);
    }

    /// <summary>
    /// Delete old audit logs (Admin only)
    /// </summary>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOldAuditLogs([FromQuery] int daysToKeep = 365)
    {
        await _auditLogService.DeleteOldAuditLogsAsync(daysToKeep);
        return Ok(new { message = $"Audit logs older than {daysToKeep} days deleted successfully" });
    }
}
