using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/gift-cards")]
[Authorize]
public class GiftCardsController : BaseController
{
    private readonly IGiftCardService _giftCardService;
        public GiftCardsController(IGiftCardService giftCardService)
    {
        _giftCardService = giftCardService;
            }

    /// <summary>
    /// Kullanıcının hediye kartlarını getirir (pagination ile)
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<GiftCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<GiftCardDto>>> GetMyGiftCards(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var giftCards = await _giftCardService.GetUserGiftCardsAsync(userId, page, pageSize, cancellationToken);
        return Ok(giftCards);
    }

    /// <summary>
    /// Hediye kartı detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var giftCard = await _giftCardService.GetByIdAsync(id, cancellationToken);
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
    [HttpGet("code/{code}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var giftCard = await _giftCardService.GetByCodeAsync(code, cancellationToken);
        if (giftCard == null)
        {
            return NotFound();
        }
        return Ok(giftCard);
    }

    /// <summary>
    /// Hediye kartı satın alır
    /// </summary>
    [HttpPost("purchase")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika (kritik işlem)
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> Purchase(
        [FromBody] PurchaseGiftCardDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var giftCard = await _giftCardService.PurchaseGiftCardAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = giftCard.Id }, giftCard);
    }

    /// <summary>
    /// Hediye kartını kullanır
    /// </summary>
    [HttpPost("redeem/{code}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika (kritik işlem)
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> Redeem(
        string code,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var giftCard = await _giftCardService.RedeemGiftCardAsync(code, userId, cancellationToken);
        return Ok(giftCard);
    }

    /// <summary>
    /// Hediye kartı indirim miktarını hesaplar
    /// </summary>
    [HttpPost("calculate-discount")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateDiscount(
        [FromQuery] string code,
        [FromQuery] decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var discount = await _giftCardService.CalculateDiscountAsync(code, orderAmount, cancellationToken);
        return Ok(new { discount });
    }
}

