using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/wishlist")]
[Authorize]
public class WishlistController : BaseController
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetWishlist()
    {
        var userId = GetUserId();
        var products = await _wishlistService.GetWishlistAsync(userId);
        return Ok(products);
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToWishlist(Guid productId)
    {
        var userId = GetUserId();
        var result = await _wishlistService.AddToWishlistAsync(userId, productId);
        if (!result)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId)
    {
        var userId = GetUserId();
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("{productId}/check")]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId)
    {
        var userId = GetUserId();
        var result = await _wishlistService.IsInWishlistAsync(userId, productId);
        return Ok(new { isInWishlist = result });
    }
}

