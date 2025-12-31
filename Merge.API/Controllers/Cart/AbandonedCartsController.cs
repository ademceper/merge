using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/abandoned")]
public class AbandonedCartsController : BaseController
{
    private readonly IAbandonedCartService _abandonedCartService;

    public AbandonedCartsController(IAbandonedCartService abandonedCartService)
    {
        _abandonedCartService = abandonedCartService;
    }

    /// <summary>
    /// Get all abandoned carts
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AbandonedCartDto>>> GetAbandonedCarts(
        [FromQuery] int minHours = 1,
        [FromQuery] int maxDays = 30)
    {
        var carts = await _abandonedCartService.GetAbandonedCartsAsync(minHours, maxDays);
        return Ok(carts);
    }

    /// <summary>
    /// Get abandoned cart by ID
    /// </summary>
    [HttpGet("{cartId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AbandonedCartDto>> GetAbandonedCartById(Guid cartId)
    {
        var cart = await _abandonedCartService.GetAbandonedCartByIdAsync(cartId);

        if (cart == null)
        {
            return NotFound();
        }

        return Ok(cart);
    }

    /// <summary>
    /// Get abandoned cart recovery statistics
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AbandonedCartRecoveryStatsDto>> GetRecoveryStats([FromQuery] int days = 30)
    {
        var stats = await _abandonedCartService.GetRecoveryStatsAsync(days);
        return Ok(stats);
    }

    /// <summary>
    /// Send recovery email to a specific cart
    /// </summary>
    [HttpPost("{cartId}/send-email")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SendRecoveryEmail(Guid cartId, [FromBody] SendRecoveryEmailDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _abandonedCartService.SendRecoveryEmailAsync(
            cartId,
            dto.EmailType,
            dto.IncludeCoupon,
            dto.CouponDiscountPercentage);

        return NoContent();
    }

    /// <summary>
    /// Send bulk recovery emails to all eligible abandoned carts
    /// </summary>
    [HttpPost("send-bulk-emails")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulkRecoveryEmails(
        [FromQuery] int minHours = 2,
        [FromQuery] string emailType = "First")
    {
        await _abandonedCartService.SendBulkRecoveryEmailsAsync(minHours, emailType);
        return NoContent();
    }

    /// <summary>
    /// Track email open (for email tracking pixels)
    /// </summary>
    [HttpGet("track/open/{emailId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackEmailOpen(Guid emailId)
    {
        await _abandonedCartService.TrackEmailOpenAsync(emailId);

        // Return a 1x1 transparent pixel
        var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// Track email click
    /// </summary>
    [HttpGet("track/click/{emailId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackEmailClick(Guid emailId)
    {
        await _abandonedCartService.TrackEmailClickAsync(emailId);
        return Redirect("/cart"); // Redirect to cart page
    }

    /// <summary>
    /// Mark cart as recovered (called when user completes purchase)
    /// </summary>
    [HttpPost("{cartId}/mark-recovered")]
    [Authorize]
    public async Task<IActionResult> MarkCartAsRecovered(Guid cartId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: Authorization check - Users can only mark their own carts as recovered
        var cart = await _abandonedCartService.GetAbandonedCartByIdAsync(cartId);
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        await _abandonedCartService.MarkCartAsRecoveredAsync(cartId);
        return NoContent();
    }

    /// <summary>
    /// Get email history for a specific cart
    /// </summary>
    [HttpGet("{cartId}/email-history")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AbandonedCartEmailDto>>> GetCartEmailHistory(Guid cartId)
    {
        var history = await _abandonedCartService.GetCartEmailHistoryAsync(cartId);
        return Ok(history);
    }
}
