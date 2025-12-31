using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Product;


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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetRecentlyViewed([FromQuery] int count = 20)
    {
        var userId = GetUserId();
        var products = await _recentlyViewedService.GetRecentlyViewedAsync(userId, count);
        return Ok(products);
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToRecentlyViewed(Guid productId)
    {
        var userId = GetUserId();
        await _recentlyViewedService.AddToRecentlyViewedAsync(userId, productId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearRecentlyViewed()
    {
        var userId = GetUserId();
        await _recentlyViewedService.ClearRecentlyViewedAsync(userId);
        return NoContent();
    }
}

