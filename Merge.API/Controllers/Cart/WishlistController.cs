using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;


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
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetWishlist(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var products = await _wishlistService.GetWishlistAsync(userId, page, pageSize, cancellationToken);
        return Ok(products);
    }

    [HttpPost("{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.AddToWishlistAsync(userId, productId, cancellationToken);
        if (!result)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpDelete("{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("{productId}/check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.IsInWishlistAsync(userId, productId, cancellationToken);
        return Ok(new { isInWishlist = result });
    }
}

