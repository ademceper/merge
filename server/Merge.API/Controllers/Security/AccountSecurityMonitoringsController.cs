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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Security;

/// <summary>
/// Account Security Monitoring Controller - Güvenlik olaylarını ve uyarılarını yönetir
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/security/account-monitoring")]
[Authorize]
public class AccountSecurityMonitoringsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;
    
    public AccountSecurityMonitoringsController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Güvenlik olayı kaydeder
    /// </summary>
    /// <param name="dto">Güvenlik olayı bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan güvenlik olayı</returns>
    /// <response code="201">Güvenlik olayı başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("events")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (Security event logging koruması)
    [ProducesResponseType(typeof(AccountSecurityEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AccountSecurityEventDto>> LogSecurityEvent(
        [FromBody] CreateAccountSecurityEventDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        if (string.IsNullOrEmpty(dto.IpAddress))
        {
            dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        if (string.IsNullOrEmpty(dto.UserAgent))
        {
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
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
        
        var securityEvent = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUserSecurityEvents), new { userId = securityEvent.UserId }, securityEvent);
    }

    /// <summary>
    /// Kullanıcının güvenlik olaylarını getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="eventType">Olay tipi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu (max 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış güvenlik olayları listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("events/user/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - ✅ BOLUM 12.0: Magic number config'den
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserSecurityEventsQuery(userId, eventType, page, pageSize);
        var events = await _mediator.Send(query, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Şüpheli güvenlik olaylarını getirir
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu (max 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış şüpheli güvenlik olayları listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("events/suspicious")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<AccountSecurityEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<AccountSecurityEventDto>>> GetSuspiciousEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - ✅ BOLUM 12.0: Magic number config'den
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSuspiciousEventsQuery(page, pageSize);
        var events = await _mediator.Send(query, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Güvenlik olayı için aksiyon alır
    /// </summary>
    /// <param name="eventId">Güvenlik olayı ID</param>
    /// <param name="dto">Aksiyon bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Aksiyon başarıyla alındı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Güvenlik olayı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("events/{eventId}/action")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var actionTakenByUserId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new TakeActionCommand(eventId, actionTakenByUserId, dto.Action, dto.Notes);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Güvenlik uyarısı oluşturur
    /// </summary>
    /// <param name="dto">Güvenlik uyarısı bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan güvenlik uyarısı</returns>
    /// <response code="201">Güvenlik uyarısı başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(SecurityAlertDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SecurityAlertDto>> CreateSecurityAlert(
        [FromBody] CreateSecurityAlertDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateSecurityAlertCommand(
            dto.UserId,
            dto.AlertType,
            dto.Severity,
            dto.Title,
            dto.Description,
            dto.Metadata);
        
        var alert = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSecurityAlerts), new { userId = alert.UserId }, alert);
    }

    /// <summary>
    /// Güvenlik uyarılarını getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID (opsiyonel)</param>
    /// <param name="severity">Önem seviyesi (opsiyonel)</param>
    /// <param name="status">Durum (opsiyonel)</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu (max 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış güvenlik uyarıları listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - ✅ BOLUM 12.0: Magic number config'den
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSecurityAlertsQuery(userId, severity, status, page, pageSize);
        var alerts = await _mediator.Send(query, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Güvenlik uyarısını onaylar
    /// </summary>
    /// <param name="alertId">Güvenlik uyarısı ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Uyarı başarıyla onaylandı</response>
    /// <response code="404">Güvenlik uyarısı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("alerts/{alertId}/acknowledge")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AcknowledgeAlert(
        Guid alertId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var acknowledgedByUserId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new AcknowledgeAlertCommand(alertId, acknowledgedByUserId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Güvenlik uyarısını çözer
    /// </summary>
    /// <param name="alertId">Güvenlik uyarısı ID</param>
    /// <param name="dto">Çözüm bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Uyarı başarıyla çözüldü</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Güvenlik uyarısı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("alerts/{alertId}/resolve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var resolvedByUserId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ResolveAlertCommand(alertId, resolvedByUserId, dto.ResolutionNotes);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Güvenlik izleme özetini getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güvenlik izleme özeti</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SecurityMonitoringSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SecurityMonitoringSummaryDto>> GetSecuritySummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSecuritySummaryQuery(startDate, endDate);
        var summary = await _mediator.Send(query, cancellationToken);
        return Ok(summary);
    }
}

