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
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Marketing;

/// <summary>
/// Gift Cards API endpoints.
/// Hediye kartı işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/gift-cards")]
[Authorize]
[Tags("GiftCards")]
public class GiftCardsController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<GiftCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<GiftCardDto>>> GetMyGiftCards(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var userId = GetUserId();
        
        var query = new GetUserGiftCardsQuery(userId, PageNumber: page, PageSize: pageSize);
        var giftCards = await mediator.Send(query, cancellationToken);
        return Ok(giftCards);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
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

        var query = new GetGiftCardByIdQuery(id);
        var giftCard = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("GiftCard", id);

        if (giftCard.PurchasedByUserId != userId && giftCard.AssignedToUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(giftCard);
    }

    [HttpGet("code/{code}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetGiftCardByCodeQuery(code);
        var giftCard = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("GiftCard", code);

        return Ok(giftCard);
    }

    [HttpPost("purchase")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(GiftCardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<GiftCardDto>> Purchase(
        [FromBody] PurchaseGiftCardDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        var command = new PurchaseGiftCardCommand(
            userId,
            dto.Amount,
            dto.AssignedToUserId,
            dto.Message,
            dto.ExpiresAt);
        
        var giftCard = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = giftCard.Id }, giftCard);
    }

    [HttpPost("redeem/{code}")]
    [RateLimit(10, 60)]
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
        
        var command = new RedeemGiftCardCommand(code, userId);
        var giftCard = await mediator.Send(command, cancellationToken);
        return Ok(giftCard);
    }

    /// <summary>
    /// Hediye kartı indirimini hesaplar
    /// </summary>
    /// <param name="code">Hediye kartı kodu</param>
    /// <param name="orderAmount">Sipariş tutarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İndirim tutarı</returns>
    /// <response code="200">İndirim başarıyla hesaplandı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("calculate-discount")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateDiscount(
        [FromQuery] string code,
        [FromQuery] decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        var query = new CalculateGiftCardDiscountQuery(code, orderAmount);
        var discount = await mediator.Send(query, cancellationToken);
        return Ok(discount);
    }
}
