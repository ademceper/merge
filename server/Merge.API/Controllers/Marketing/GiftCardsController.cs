using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetUserGiftCards;
using Merge.Application.Marketing.Queries.GetGiftCardById;
using Merge.Application.Marketing.Queries.GetGiftCardByCode;
using Merge.Application.Marketing.Queries.CalculateGiftCardDiscount;
using Merge.Application.Marketing.Commands.PurchaseGiftCard;
using Merge.Application.Marketing.Commands.RedeemGiftCard;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/gift-cards")]
[Authorize]
public class GiftCardsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MarketingSettings _marketingSettings;

    public GiftCardsController(
        IMediator mediator,
        IOptions<MarketingSettings> marketingSettings)
    {
        _mediator = mediator;
        _marketingSettings = marketingSettings.Value;
    }

    /// <summary>
    /// Kullanıcının hediye kartlarını getirir (pagination ile)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Hediye kartı listesi</returns>
    /// <response code="200">Hediye kartları başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kullanıcının hediye kartlarını getirir",
        Description = "Sayfalama ile kullanıcının hediye kartlarını getirir (satın aldığı veya kendisine atanan).")]
    [ProducesResponseType(typeof(PagedResult<GiftCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<GiftCardDto>>> GetMyGiftCards(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var userId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetUserGiftCardsQuery(userId, PageNumber: page, PageSize: pageSize);
        var giftCards = await _mediator.Send(query, cancellationToken);
        return Ok(giftCards);
    }

    /// <summary>
    /// Hediye kartı detaylarını getirir
    /// </summary>
    /// <param name="id">Hediye kartı ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Hediye kartı detayları</returns>
    /// <response code="200">Hediye kartı başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu hediye kartına erişim yetkiniz yok</response>
    /// <response code="404">Hediye kartı bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Hediye kartı detaylarını getirir",
        Description = "Hediye kartı ID'sine göre hediye kartı detaylarını getirir. Kullanıcı sadece kendi hediye kartlarına erişebilir.")]
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetGiftCardByIdQuery(id);
        var giftCard = await _mediator.Send(query, cancellationToken);
        
        if (giftCard == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi gift card'larına erişebilmeli
        if (giftCard.PurchasedByUserId != userId && giftCard.AssignedToUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(giftCard);
    }

    /// <summary>
    /// Hediye kartı koduna göre getirir
    /// </summary>
    /// <param name="code">Hediye kartı kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Hediye kartı detayları</returns>
    /// <response code="200">Hediye kartı başarıyla getirildi</response>
    /// <response code="404">Hediye kartı bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("code/{code}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Hediye kartı koduna göre getirir",
        Description = "Hediye kartı koduna göre hediye kartı detaylarını getirir.")]
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetGiftCardByCodeQuery(code);
        var giftCard = await _mediator.Send(query, cancellationToken);
        
        if (giftCard == null)
        {
            return NotFound();
        }
        
        return Ok(giftCard);
    }

    /// <summary>
    /// Hediye kartı satın alır
    /// </summary>
    /// <param name="dto">Hediye kartı satın alma bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Satın alınan hediye kartı</returns>
    /// <response code="201">Hediye kartı başarıyla satın alındı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("purchase")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika (kritik işlem)
    [SwaggerOperation(
        Summary = "Hediye kartı satın alır",
        Description = "Yeni bir hediye kartı satın alır. Hediye kartı kodu otomatik oluşturulur.")]
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> Purchase(
        [FromBody] PurchaseGiftCardDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new PurchaseGiftCardCommand(
            userId,
            dto.Amount,
            dto.AssignedToUserId,
            dto.Message,
            dto.ExpiresAt);
        
        var giftCard = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = giftCard.Id }, giftCard);
    }

    /// <summary>
    /// Hediye kartını kullanır
    /// </summary>
    /// <param name="code">Hediye kartı kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanılan hediye kartı</returns>
    /// <response code="200">Hediye kartı başarıyla kullanıldı</response>
    /// <response code="400">Hediye kartı kullanılamaz durumda</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="404">Hediye kartı bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("redeem/{code}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika (kritik işlem)
    [SwaggerOperation(
        Summary = "Hediye kartını kullanır",
        Description = "Hediye kartı koduna göre hediye kartını kullanır (tam bakiyeyi kullanır).")]
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> Redeem(
        string code,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new RedeemGiftCardCommand(code, userId);
        var giftCard = await _mediator.Send(command, cancellationToken);
        return Ok(giftCard);
    }

    /// <summary>
    /// Hediye kartı indirim miktarını hesaplar
    /// </summary>
    /// <param name="code">Hediye kartı kodu</param>
    /// <param name="orderAmount">Sipariş tutarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İndirim miktarı</returns>
    /// <response code="200">İndirim miktarı hesaplandı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("calculate-discount")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [SwaggerOperation(
        Summary = "Hediye kartı indirim miktarını hesaplar",
        Description = "Hediye kartı koduna ve sipariş tutarına göre indirim miktarını hesaplar.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateDiscount(
        [FromQuery] string code,
        [FromQuery] decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new CalculateGiftCardDiscountQuery(code, orderAmount);
        var discount = await _mediator.Send(query, cancellationToken);
        return Ok(new { discount });
    }
}
