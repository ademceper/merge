using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Seller;


namespace Merge.API.Controllers.Seller;

[ApiController]
[Route("api/seller/dashboard")]
[Authorize(Roles = "Seller,Admin")]
public class DashboardController : BaseController
{
    private readonly ISellerDashboardService _sellerDashboardService;
        public DashboardController(ISellerDashboardService sellerDashboardService)
    {
        _sellerDashboardService = sellerDashboardService;
            }

    [HttpGet("stats")]
    public async Task<ActionResult<SellerDashboardStatsDto>> GetStats()
    {
        var sellerId = GetUserId();
        var stats = await _sellerDashboardService.GetDashboardStatsAsync(sellerId);
        return Ok(stats);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var sellerId = GetUserId();
        var orders = await _sellerDashboardService.GetSellerOrdersAsync(sellerId, page, pageSize);
        return Ok(orders);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var sellerId = GetUserId();
        var products = await _sellerDashboardService.GetSellerProductsAsync(sellerId, page, pageSize);
        return Ok(products);
    }

    [HttpGet("performance")]
    public async Task<ActionResult<SellerPerformanceDto>> GetPerformance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var sellerId = GetUserId();
        var performance = await _sellerDashboardService.GetPerformanceMetricsAsync(sellerId, startDate, endDate);
        return Ok(performance);
    }

    [HttpGet("performance/detailed")]
    public async Task<ActionResult<SellerPerformanceMetricsDto>> GetDetailedPerformance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var sellerId = GetUserId();
        var performance = await _sellerDashboardService.GetDetailedPerformanceMetricsAsync(sellerId, startDate, endDate);
        return Ok(performance);
    }

    [HttpGet("performance/categories")]
    public async Task<ActionResult<List<CategoryPerformanceDto>>> GetCategoryPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var sellerId = GetUserId();
        var performance = await _sellerDashboardService.GetCategoryPerformanceAsync(sellerId, startDate, endDate);
        return Ok(performance);
    }
}

