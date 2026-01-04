using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Governance;
using Merge.Application.DTOs.Security;
using Merge.API.Middleware;

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
    /// Audit log entry oluşturur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (audit logging için yüksek limit)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateAuditLog(
        [FromBody] CreateAuditLogDto auditDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _auditLogService.LogAsync(auditDto, ipAddress, userAgent, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Audit log detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuditLogDto>> GetAuditById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var audit = await _auditLogService.GetAuditByIdAsync(id, cancellationToken);

        if (audit == null)
        {
            return NotFound();
        }

        return Ok(audit);
    }

    /// <summary>
    /// Audit log'ları filtreleyerek getirir (sayfalanmış)
    /// </summary>
    [HttpPost("search")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> SearchAuditLogs(
        [FromBody] AuditLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (filter.PageSize > 100) filter.PageSize = 100;
        if (filter.PageNumber < 1) filter.PageNumber = 1;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var audits = await _auditLogService.GetAuditLogsAsync(filter, cancellationToken);
        return Ok(audits);
    }

    /// <summary>
    /// Entity audit geçmişini getirir
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(EntityAuditHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EntityAuditHistoryDto>> GetEntityHistory(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var history = await _auditLogService.GetEntityHistoryAsync(entityType, entityId, cancellationToken);

        if (history == null)
        {
            return NotFound();
        }

        return Ok(history);
    }

    /// <summary>
    /// Audit istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(AuditStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuditStatsDto>> GetAuditStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _auditLogService.GetAuditStatsAsync(days, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Kullanıcı audit geçmişini getirir
    /// </summary>
    [HttpGet("user/{userId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetUserAuditHistory(
        Guid userId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var audits = await _auditLogService.GetUserAuditHistoryAsync(userId, days, cancellationToken);
        return Ok(audits);
    }

    /// <summary>
    /// Kendi audit geçmişini getirir
    /// </summary>
    [HttpGet("my-history")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetMyAuditHistory(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi audit geçmişini görebilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var audits = await _auditLogService.GetUserAuditHistoryAsync(userId, days, cancellationToken);
        return Ok(audits);
    }

    /// <summary>
    /// Audit log değişikliklerini karşılaştırır
    /// </summary>
    [HttpGet("{auditLogId}/compare")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<AuditComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AuditComparisonDto>>> CompareChanges(
        Guid auditLogId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var comparisons = await _auditLogService.CompareChangesAsync(auditLogId, cancellationToken);
        return Ok(comparisons);
    }

    /// <summary>
    /// Eski audit log'ları siler (Admin only)
    /// </summary>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (tehlikeli işlem)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteOldAuditLogs(
        [FromQuery] int daysToKeep = 365,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _auditLogService.DeleteOldAuditLogsAsync(daysToKeep, cancellationToken);
        return Ok(new { message = $"Audit logs older than {daysToKeep} days deleted successfully" });
    }
}
