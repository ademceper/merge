using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;


namespace Merge.API.Controllers.Analytics;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;
        public AdminController(
        IAdminService adminService)
    {
        _adminService = adminService;
            }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpGet("dashboard/revenue-chart")]
    public async Task<ActionResult<RevenueChartDto>> GetRevenueChart([FromQuery] int days = 30)
    {
        var chart = await _adminService.GetRevenueChartAsync(days);
        return Ok(chart);
    }

    [HttpGet("dashboard/top-products")]
    public async Task<ActionResult<IEnumerable<AdminTopProductDto>>> GetTopProducts([FromQuery] int count = 10)
    {
        var topProducts = await _adminService.GetTopProductsAsync(count);
        return Ok(topProducts);
    }

    [HttpGet("dashboard/inventory-overview")]
    public async Task<ActionResult<InventoryOverviewDto>> GetInventoryOverview()
    {
        var overview = await _adminService.GetInventoryOverviewAsync();
        return Ok(overview);
    }

    [HttpGet("orders/recent")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRecentOrders([FromQuery] int count = 10)
    {
        var orders = await _adminService.GetRecentOrdersAsync(count);
        return Ok(orders);
    }

    [HttpGet("products/low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        var products = await _adminService.GetLowStockProductsAsync(threshold);
        return Ok(products);
    }

    [HttpGet("reviews/pending")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetPendingReviews()
    {
        var reviews = await _adminService.GetPendingReviewsAsync();
        return Ok(reviews);
    }

    [HttpGet("returns/pending")]
    public async Task<ActionResult<IEnumerable<ReturnRequestDto>>> GetPendingReturns()
    {
        var returns = await _adminService.GetPendingReturnsAsync();
        return Ok(returns);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null)
    {
        var users = await _adminService.GetUsersAsync(page, pageSize, role);
        return Ok(users);
    }

    [HttpPost("users/{userId}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        var result = await _adminService.ActivateUserAsync(userId);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpPost("users/{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        var result = await _adminService.DeactivateUserAsync(userId);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpPost("users/{userId}/change-role")]
    public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeRoleDto roleDto)
    {
        var result = await _adminService.ChangeUserRoleAsync(userId, roleDto.Role);
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { message = $"User role changed to {roleDto.Role}" });
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var result = await _adminService.DeleteUserAsync(userId);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpGet("analytics/summary")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetAnalyticsSummary([FromQuery] int days = 30)
    {
        var summary = await _adminService.GetAnalyticsSummaryAsync(days);
        return Ok(summary);
    }

    [HttpGet("security/2fa-stats")]
    public async Task<ActionResult<TwoFactorStatsDto>> Get2FAStats()
    {
        var stats = await _adminService.Get2FAStatsAsync();
        return Ok(stats);
    }

    [HttpGet("system/health")]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
    {
        var health = await _adminService.GetSystemHealthAsync();
        return Ok(health);
    }
}
