using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;


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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GiftCardDto>>> GetMyGiftCards()
    {
        var userId = GetUserId();
        var giftCards = await _giftCardService.GetUserGiftCardsAsync(userId);
        return Ok(giftCards);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GiftCardDto>> GetById(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var giftCard = await _giftCardService.GetByIdAsync(id);
        if (giftCard == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi gift card'larına erişebilmeli (satın aldığı veya kendisine atanan)
        if (giftCard.PurchasedByUserId != userId && giftCard.AssignedToUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(giftCard);
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<GiftCardDto>> GetByCode(string code)
    {
        var giftCard = await _giftCardService.GetByCodeAsync(code);
        if (giftCard == null)
        {
            return NotFound();
        }
        return Ok(giftCard);
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<GiftCardDto>> Purchase([FromBody] PurchaseGiftCardDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var giftCard = await _giftCardService.PurchaseGiftCardAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = giftCard.Id }, giftCard);
    }

    [HttpPost("redeem/{code}")]
    public async Task<ActionResult<GiftCardDto>> Redeem(string code)
    {
        var userId = GetUserId();
        var giftCard = await _giftCardService.RedeemGiftCardAsync(code, userId);
        return Ok(giftCard);
    }

    [HttpPost("calculate-discount")]
    public async Task<ActionResult<decimal>> CalculateDiscount([FromQuery] string code, [FromQuery] decimal orderAmount)
    {
        var discount = await _giftCardService.CalculateDiscountAsync(code, orderAmount);
        return Ok(new { discount });
    }
}

