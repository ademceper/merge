using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Security.Commands.CreateOrderVerification;
using Merge.Application.Security.Commands.VerifyOrder;
using Merge.Application.Security.Commands.RejectOrder;
using Merge.Application.Security.Queries.GetVerificationByOrderId;
using Merge.Application.Security.Queries.GetPendingVerifications;
using Merge.Application.Security.Queries.GetAllVerifications;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Security;

/// <summary>
/// Order Verification Controller - Sipariş doğrulama işlemlerini yönetir
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/security/order-verifications")]
[Authorize]
public class OrderVerificationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;
    
    public OrderVerificationsController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Sipariş doğrulama kaydı oluşturur
    /// </summary>
    /// <param name="dto">Doğrulama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan doğrulama kaydı</returns>
    /// <response code="201">Doğrulama kaydı başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(OrderVerificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderVerificationDto>> CreateVerification(
        [FromBody] CreateOrderVerificationDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateOrderVerificationCommand(
            dto.OrderId,
            dto.VerificationType,
            dto.VerificationMethod,
            dto.VerificationNotes,
            dto.RequiresManualReview);
        
        var verification = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVerificationByOrderId), new { orderId = verification.OrderId }, verification);
    }

    /// <summary>
    /// Sipariş ID'sine göre doğrulama kaydını getirir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Doğrulama kaydı</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="404">Doğrulama kaydı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("order/{orderId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(OrderVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderVerificationDto>> GetVerificationByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetVerificationByOrderIdQuery(orderId);
        var verification = await _mediator.Send(query, cancellationToken);
        
        if (verification == null)
        {
            return NotFound();
        }
        return Ok(verification);
    }

    /// <summary>
    /// Bekleyen doğrulama kayıtlarını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Bekleyen doğrulama kayıtları listesi</returns>
    /// <response code="200">Başarılı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<OrderVerificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<OrderVerificationDto>>> GetPendingVerifications(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPendingVerificationsQuery();
        var verifications = await _mediator.Send(query, cancellationToken);
        return Ok(verifications);
    }

    /// <summary>
    /// Tüm doğrulama kayıtlarını getirir
    /// </summary>
    /// <param name="status">Durum (opsiyonel)</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu (max 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış doğrulama kayıtları listesi</returns>
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
    [ProducesResponseType(typeof(PagedResult<OrderVerificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<OrderVerificationDto>>> GetAllVerifications(
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
        var query = new GetAllVerificationsQuery(status, page, pageSize);
        var verifications = await _mediator.Send(query, cancellationToken);
        return Ok(verifications);
    }

    /// <summary>
    /// Siparişi doğrular
    /// </summary>
    /// <param name="verificationId">Doğrulama kaydı ID</param>
    /// <param name="dto">Doğrulama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Sipariş başarıyla doğrulandı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Doğrulama kaydı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{verificationId}/verify")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyOrder(
        Guid verificationId,
        [FromBody] VerifyOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var verifiedByUserId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new VerifyOrderCommand(verificationId, verifiedByUserId, dto.Notes);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Siparişi reddeder
    /// </summary>
    /// <param name="verificationId">Doğrulama kaydı ID</param>
    /// <param name="dto">Reddetme bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı işlem</returns>
    /// <response code="204">Sipariş başarıyla reddedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Doğrulama kaydı bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Yetki yetersiz</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{verificationId}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RejectOrder(
        Guid verificationId,
        [FromBody] RejectOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var verifiedByUserId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new RejectOrderCommand(verificationId, verifiedByUserId, dto.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

