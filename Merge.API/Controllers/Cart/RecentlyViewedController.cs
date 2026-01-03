using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/recently-viewed")]
[Authorize]
public class RecentlyViewedController : BaseController
{
    private readonly IRecentlyViewedService _recentlyViewedService;

    public RecentlyViewedController(IRecentlyViewedService recentlyViewedService)
    {
        _recentlyViewedService = recentlyViewedService;
    }

    /// <summary>
    /// Son görüntülenen ürünleri getirir
    /// </summary>
    [HttpGet]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetRecentlyViewed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var products = await _recentlyViewedService.GetRecentlyViewedAsync(userId, page, pageSize, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Ürünü son görüntülenenlere ekler
    /// </summary>
    [HttpPost("{productId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (yüksek limit - tracking için)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddToRecentlyViewed(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        await _recentlyViewedService.AddToRecentlyViewedAsync(userId, productId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Son görüntülenen ürünleri temizler
    /// </summary>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearRecentlyViewed(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        await _recentlyViewedService.ClearRecentlyViewedAsync(userId, cancellationToken);
        return NoContent();
    }
}

