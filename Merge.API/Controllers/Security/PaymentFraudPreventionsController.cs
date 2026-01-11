using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Security.Commands.CreatePaymentFraudCheck;
using Merge.Application.Security.Commands.BlockPayment;
using Merge.Application.Security.Commands.UnblockPayment;
using Merge.Application.Security.Queries.GetCheckByPaymentId;
using Merge.Application.Security.Queries.GetBlockedPayments;
using Merge.Application.Security.Queries.GetAllChecks;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Security;

/// <summary>
/// Payment Fraud Prevention Controller - Ödeme dolandırıcılık önleme işlemlerini yönetir
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/security/payment-fraud-prevention")]
[Authorize]
public class PaymentFraudPreventionsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;
    
    public PaymentFraudPreventionsController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Ödeme dolandırıcılık kontrolü yapar
    /// </summary>
    /// <param name="dto">Kontrol bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kontrol kaydı</returns>
    /// <response code="201">Kontrol kaydı başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("check")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (Fraud check koruması)
    [ProducesResponseType(typeof(PaymentFraudPreventionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentFraudPreventionDto>> CheckPayment(
        [FromBody] CreatePaymentFraudCheckDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var ipAddress = string.IsNullOrEmpty(dto.IpAddress)
            ? HttpContext.Connection.RemoteIpAddress?.ToString()
            : dto.IpAddress;
        
        var userAgent = string.IsNullOrEmpty(dto.UserAgent)
            ? Request.Headers["User-Agent"].ToString()
            : dto.UserAgent;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreatePaymentFraudCheckCommand(
            dto.PaymentId,
            dto.CheckType,
            dto.DeviceFingerprint,
            ipAddress,
            userAgent);
        
        var check = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCheckByPaymentId), new { paymentId = dto.PaymentId }, check);
    }

    /// <summary>
    /// Ödeme ID'sine göre kontrol kaydını getirir
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kontrol kaydı</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="404">Kontrol kaydı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("payment/{paymentId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PaymentFraudPreventionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentFraudPreventionDto>> GetCheckByPaymentId(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetCheckByPaymentIdQuery(paymentId);
        var check = await _mediator.Send(query, cancellationToken);
        
        if (check == null)
        {
            return NotFound();
        }
        return Ok(check);
    }

    /// <summary>
    /// Engellenen ödemeleri getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Engellenen ödemeler listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("blocked")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<PaymentFraudPreventionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PaymentFraudPreventionDto>>> GetBlockedPayments(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlockedPaymentsQuery();
        var checks = await _mediator.Send(query, cancellationToken);
        return Ok(checks);
    }

    /// <summary>
    /// Tüm kontrol kayıtlarını getirir
    /// </summary>
    /// <param name="status">Durum (opsiyonel)</param>
    /// <param name="isBlocked">Engellenmiş mi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu (max 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış kontrol kayıtları listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<PaymentFraudPreventionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PaymentFraudPreventionDto>>> GetAllChecks(
        [FromQuery] string? status = null,
        [FromQuery] bool? isBlocked = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - ✅ BOLUM 12.0: Magic number config'den
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAllChecksQuery(status, isBlocked, page, pageSize);
        var checks = await _mediator.Send(query, cancellationToken);
        return Ok(checks);
    }

    /// <summary>
    /// Ödemeyi engeller
    /// </summary>
    /// <param name="checkId">Kontrol kaydı ID</param>
    /// <param name="dto">Engelleme bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Ödeme başarıyla engellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Kontrol kaydı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{checkId}/block")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BlockPayment(
        Guid checkId,
        [FromBody] BlockPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new BlockPaymentCommand(checkId, dto.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Ödeme engelini kaldırır
    /// </summary>
    /// <param name="checkId">Kontrol kaydı ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Ödeme engeli başarıyla kaldırıldı</response>
    /// <response code="404">Kontrol kaydı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{checkId}/unblock")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnblockPayment(
        Guid checkId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UnblockPaymentCommand(checkId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

