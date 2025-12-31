using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Review;
using Merge.Application.DTOs.Review;


namespace Merge.API.Controllers.Review;

[ApiController]
[Route("api/reviews/trust-badges")]
public class TrustBadgesController : BaseController
{
    private readonly ITrustBadgeService _trustBadgeService;

    public TrustBadgesController(ITrustBadgeService trustBadgeService)
    {
        _trustBadgeService = trustBadgeService;
    }

    // Badge Management (Admin only)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrustBadgeDto>> CreateBadge([FromBody] CreateTrustBadgeDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var badge = await _trustBadgeService.CreateBadgeAsync(dto);
        return CreatedAtAction(nameof(GetBadge), new { id = badge.Id }, badge);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrustBadgeDto>> GetBadge(Guid id)
    {
        var badge = await _trustBadgeService.GetBadgeAsync(id);
        if (badge == null)
        {
            return NotFound();
        }
        return Ok(badge);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrustBadgeDto>>> GetBadges([FromQuery] string? badgeType = null)
    {
        var badges = await _trustBadgeService.GetBadgesAsync(badgeType);
        return Ok(badges);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrustBadgeDto>> UpdateBadge(Guid id, [FromBody] UpdateTrustBadgeDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var badge = await _trustBadgeService.UpdateBadgeAsync(id, dto);
        if (badge == null)
        {
            return NotFound();
        }
        return Ok(badge);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBadge(Guid id)
    {
        var success = await _trustBadgeService.DeleteBadgeAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Seller Badges
    [HttpPost("seller/{sellerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SellerTrustBadgeDto>> AwardSellerBadge(Guid sellerId, [FromBody] AwardBadgeDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var badge = await _trustBadgeService.AwardSellerBadgeAsync(sellerId, dto);
        return CreatedAtAction(nameof(GetSellerBadges), new { sellerId = sellerId }, badge);
    }

    [HttpGet("seller/{sellerId}")]
    public async Task<ActionResult<IEnumerable<SellerTrustBadgeDto>>> GetSellerBadges(Guid sellerId)
    {
        var badges = await _trustBadgeService.GetSellerBadgesAsync(sellerId);
        return Ok(badges);
    }

    [HttpDelete("seller/{sellerId}/{badgeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevokeSellerBadge(Guid sellerId, Guid badgeId)
    {
        var success = await _trustBadgeService.RevokeSellerBadgeAsync(sellerId, badgeId);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Product Badges
    [HttpPost("product/{productId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductTrustBadgeDto>> AwardProductBadge(Guid productId, [FromBody] AwardBadgeDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var badge = await _trustBadgeService.AwardProductBadgeAsync(productId, dto);
        return CreatedAtAction(nameof(GetProductBadges), new { productId = productId }, badge);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductTrustBadgeDto>>> GetProductBadges(Guid productId)
    {
        var badges = await _trustBadgeService.GetProductBadgesAsync(productId);
        return Ok(badges);
    }

    [HttpDelete("product/{productId}/{badgeId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevokeProductBadge(Guid productId, Guid badgeId)
    {
        var success = await _trustBadgeService.RevokeProductBadgeAsync(productId, badgeId);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Auto Evaluation
    [HttpPost("evaluate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EvaluateBadges([FromQuery] Guid? sellerId = null)
    {
        await _trustBadgeService.EvaluateAndAwardBadgesAsync(sellerId);
        return NoContent();
    }

    [HttpPost("evaluate/seller/{sellerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EvaluateSellerBadges(Guid sellerId)
    {
        await _trustBadgeService.EvaluateSellerBadgesAsync(sellerId);
        return NoContent();
    }

    [HttpPost("evaluate/product/{productId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EvaluateProductBadges(Guid productId)
    {
        await _trustBadgeService.EvaluateProductBadgesAsync(productId);
        return NoContent();
    }
}

