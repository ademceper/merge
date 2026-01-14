using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Commands.CreatePreOrder;
using Merge.Application.Cart.Queries.GetPreOrder;
using Merge.Application.Cart.Queries.GetUserPreOrders;
using Merge.Application.Cart.Commands.CancelPreOrder;
using Merge.Application.Cart.Commands.PayPreOrderDeposit;
using Merge.Application.Cart.Commands.ConvertPreOrderToOrder;
using Merge.Application.Cart.Commands.NotifyPreOrderAvailable;
using Merge.Application.Cart.Commands.CreatePreOrderCampaign;
using Merge.Application.Cart.Queries.GetPreOrderCampaign;
using Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;
using Merge.Application.Cart.Queries.GetPreOrderCampaignsByProduct;
using Merge.Application.Cart.Commands.UpdatePreOrderCampaign;
using Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;
using Merge.Application.Cart.Queries.GetPreOrderStats;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/pre-orders")]
[Authorize]
public class PreOrdersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public PreOrdersController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Ön sipariş oluşturur
    /// </summary>
    /// <param name="dto">Ön sipariş oluşturma isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan ön sipariş</returns>
    /// <response code="201">Ön sipariş başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderDto>> CreatePreOrder(
        [FromBody] CreatePreOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new CreatePreOrderCommand(
            userId,
            dto.ProductId,
            dto.Quantity,
            dto.VariantOptions,
            dto.Notes);
        var preOrder = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPreOrder), new { id = preOrder.Id }, preOrder);
    }

    /// <summary>
    /// Ön sipariş detaylarını getirir
    /// </summary>
    /// <param name="id">Ön sipariş ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ön sipariş detayları</returns>
    /// <response code="200">Ön sipariş başarıyla getirildi</response>
    /// <response code="404">Ön sipariş bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu ön siparişe erişim yetkisi yok</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PreOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderDto>> GetPreOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetPreOrderQuery(id);
        var preOrder = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (preOrder is null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(preOrder);
    }

    /// <summary>
    /// Kullanıcının ön siparişlerini listeler
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış ön sipariş listesi</returns>
    /// <response code="200">Ön siparişler başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("my-preorders")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<PreOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderDto>>> GetMyPreOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var query = new GetUserPreOrdersQuery(userId, page, pageSize);
        var preOrders = await _mediator.Send(query, cancellationToken);
        return Ok(preOrders);
    }

    /// <summary>
    /// Ön siparişi iptal eder
    /// </summary>
    /// <param name="id">Ön sipariş ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ön sipariş başarıyla iptal edildi</response>
    /// <response code="404">Ön sipariş bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu ön siparişi iptal etme yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali (örn: zaten iptal edilmiş)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{id}")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelPreOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var preOrderQuery = new GetPreOrderQuery(id);
        var preOrder = await _mediator.Send(preOrderQuery, cancellationToken);
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (preOrder is null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new CancelPreOrderCommand(id, userId);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş depozitosu öder
    /// </summary>
    /// <param name="dto">Depozito ödeme isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Depozito başarıyla ödendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Ön sipariş bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu ön sipariş için depozito ödeme yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali (örn: yetersiz bakiye, geçersiz tutar)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("pay-deposit")]
    [RateLimit(3, 60)] // ✅ BOLUM 3.3: Rate Limiting - 3 istek / dakika (ödeme işlemleri için düşük limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PayDeposit(
        [FromBody] PayPreOrderDepositDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var preOrderQuery = new GetPreOrderQuery(dto.PreOrderId);
        var preOrder = await _mediator.Send(preOrderQuery, cancellationToken);
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (preOrder is null)
        {
            return NotFound();
        }

        if (preOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new PayPreOrderDepositCommand(userId, dto.PreOrderId, dto.Amount);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön siparişi siparişe dönüştürür (Admin/Manager only)
    /// </summary>
    /// <param name="id">Ön sipariş ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ön sipariş başarıyla siparişe dönüştürüldü</response>
    /// <response code="404">Ön sipariş bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="422">İş kuralı ihlali (örn: ön sipariş durumu uygun değil)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/convert")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConvertToOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ConvertPreOrderToOrderCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş hazır olduğunda bildirim gönderir (Admin/Manager only)
    /// </summary>
    /// <param name="id">Ön sipariş ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Bildirim başarıyla gönderildi</response>
    /// <response code="404">Ön sipariş bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/notify")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> NotifyAvailable(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new NotifyPreOrderAvailableCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Campaigns
    /// <summary>
    /// Ön sipariş kampanyası oluşturur (Admin/Manager only)
    /// </summary>
    /// <param name="dto">Kampanya oluşturma isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kampanya</returns>
    /// <response code="201">Kampanya başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("campaigns")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(typeof(PreOrderCampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderCampaignDto>> CreateCampaign(
        [FromBody] CreatePreOrderCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePreOrderCampaignCommand(
            dto.Name,
            dto.Description,
            dto.ProductId,
            dto.StartDate,
            dto.EndDate,
            dto.ExpectedDeliveryDate,
            dto.MaxQuantity,
            dto.DepositPercentage,
            dto.SpecialPrice,
            dto.NotifyOnAvailable);
        var campaign = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }

    /// <summary>
    /// Ön sipariş kampanyası detaylarını getirir
    /// </summary>
    /// <param name="id">Kampanya ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kampanya detayları</returns>
    /// <response code="200">Kampanya başarıyla getirildi</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("campaigns/{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PreOrderCampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderCampaignDto>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPreOrderCampaignQuery(id);
        var campaign = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (campaign is null)
        {
            return NotFound();
        }

        return Ok(campaign);
    }

    /// <summary>
    /// Aktif ön sipariş kampanyalarını listeler
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış aktif kampanya listesi</returns>
    /// <response code="200">Kampanyalar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("campaigns")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<PreOrderCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderCampaignDto>>> GetActiveCampaigns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetActivePreOrderCampaignsQuery(page, pageSize);
        var campaigns = await _mediator.Send(query, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Ürüne göre ön sipariş kampanyalarını listeler
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış kampanya listesi</returns>
    /// <response code="200">Kampanyalar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("campaigns/product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<PreOrderCampaignDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PreOrderCampaignDto>>> GetCampaignsByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetPreOrderCampaignsByProductQuery(productId, page, pageSize);
        var campaigns = await _mediator.Send(query, cancellationToken);
        return Ok(campaigns);
    }

    /// <summary>
    /// Ön sipariş kampanyasını günceller (Admin/Manager only)
    /// </summary>
    /// <param name="id">Kampanya ID</param>
    /// <param name="dto">Kampanya güncelleme isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Kampanya başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateCampaign(
        Guid id,
        [FromBody] CreatePreOrderCampaignDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePreOrderCampaignCommand(
            id,
            dto.Name,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.ExpectedDeliveryDate,
            dto.MaxQuantity,
            dto.DepositPercentage,
            dto.SpecialPrice);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş kampanyasını devre dışı bırakır (Admin/Manager only)
    /// </summary>
    /// <param name="id">Kampanya ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Kampanya başarıyla devre dışı bırakıldı</response>
    /// <response code="404">Kampanya bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("campaigns/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeactivateCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivatePreOrderCampaignCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Ön sipariş istatistiklerini getirir (Admin/Manager only)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ön sipariş istatistikleri</returns>
    /// <response code="200">İstatistikler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PreOrderStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PreOrderStatsDto>> GetStats(CancellationToken cancellationToken = default)
    {
        var query = new GetPreOrderStatsQuery();
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
