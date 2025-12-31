using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;


namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/referrals")]
[Authorize]
public class ReferralsController : BaseController
{
    private readonly IReferralService _referralService;

    public ReferralsController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    [HttpGet("my-code")]
    public async Task<ActionResult<ReferralCodeDto>> GetMyReferralCode()
    {
        var userId = GetUserId();
        var code = await _referralService.GetMyReferralCodeAsync(userId);
        return Ok(code);
    }

    [HttpGet("my-referrals")]
    public async Task<ActionResult<IEnumerable<ReferralDto>>> GetMyReferrals()
    {
        var userId = GetUserId();
        var referrals = await _referralService.GetMyReferralsAsync(userId);
        return Ok(referrals);
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyReferralCode([FromBody] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Referans kodu boş olamaz.");
        }

        var userId = GetUserId();
        var success = await _referralService.ApplyReferralCodeAsync(userId, code);
        return success ? NoContent() : BadRequest(new { message = "Geçersiz kod" });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ReferralStatsDto>> GetStats()
    {
        var userId = GetUserId();
        var stats = await _referralService.GetReferralStatsAsync(userId);
        return Ok(stats);
    }
}

[ApiController]
[Route("api/reviews/{reviewId}/media")]
[Authorize]
public class ReviewMediaController : BaseController
{
    private readonly IReviewMediaService _mediaService;

    public ReviewMediaController(IReviewMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost]
    public async Task<ActionResult<ReviewMediaDto>> AddMedia(Guid reviewId, [FromBody] AddMediaDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var media = await _mediaService.AddMediaToReviewAsync(reviewId, dto.Url, dto.MediaType, dto.ThumbnailUrl);
        return CreatedAtAction(nameof(GetMedia), new { reviewId = reviewId }, media);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewMediaDto>>> GetMedia(Guid reviewId)
    {
        var media = await _mediaService.GetReviewMediaAsync(reviewId);
        return Ok(media);
    }

    [HttpDelete("{mediaId}")]
    public async Task<IActionResult> DeleteMedia(Guid mediaId)
    {
        await _mediaService.DeleteReviewMediaAsync(mediaId);
        return NoContent();
    }
}

[ApiController]
[Route("api/shared-wishlists")]
public class SharedWishlistsController : BaseController
{
    private readonly ISharedWishlistService _wishlistService;

    public SharedWishlistsController(ISharedWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SharedWishlistDto>> Create([FromBody] CreateSharedWishlistDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var wishlist = await _wishlistService.CreateSharedWishlistAsync(userId, dto);
        return CreatedAtAction(nameof(GetByCode), new { shareCode = wishlist.ShareCode }, wishlist);
    }

    [HttpGet("{shareCode}")]
    public async Task<ActionResult<SharedWishlistDto>> GetByCode(string shareCode)
    {
        var wishlist = await _wishlistService.GetSharedWishlistByCodeAsync(shareCode);
        return wishlist != null ? Ok(wishlist) : NotFound();
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SharedWishlistDto>>> GetMine()
    {
        var userId = GetUserId();
        var wishlists = await _wishlistService.GetMySharedWishlistsAsync(userId);
        return Ok(wishlists);
    }

    [HttpPost("items/{itemId}/mark-purchased")]
    [Authorize]
    public async Task<IActionResult> MarkPurchased(Guid itemId)
    {
        var userId = GetUserId();
        await _wishlistService.MarkItemAsPurchasedAsync(itemId, userId);
        return NoContent();
    }
}
