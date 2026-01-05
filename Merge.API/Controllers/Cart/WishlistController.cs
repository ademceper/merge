using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.API.Middleware;


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

    /// <summary>
    /// İstek listesini getirir
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetWishlist(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var products = await _wishlistService.GetWishlistAsync(userId, page, pageSize, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// İstek listesine ürün ekler
    /// </summary>
    [HttpPost("{productId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
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

    /// <summary>
    /// İstek listesinden ürün kaldırır
    /// </summary>
    [HttpDelete("{productId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
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

    /// <summary>
    /// Ürünün istek listesinde olup olmadığını kontrol eder
    /// </summary>
    [HttpGet("{productId}/check")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60 istek / dakika (yüksek limit - sık kullanılan)
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _wishlistService.IsInWishlistAsync(userId, productId, cancellationToken);
        return Ok(new { isInWishlist = result });
    }
}

