using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;
using Merge.Application.Common;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Analytics;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Dashboard istatistiklerini getirir
    /// </summary>
    [HttpGet("dashboard/stats")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(
        CancellationToken cancellationToken = default)
    {
        var stats = await _adminService.GetDashboardStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Gelir grafiğini getirir
    /// </summary>
    [HttpGet("dashboard/revenue-chart")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(RevenueChartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RevenueChartDto>> GetRevenueChart(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 4.1: Input Validation - days 1-365 arasında olmalı
        if (days < 1 || days > 365)
        {
            ModelState.AddModelError(nameof(days), "Gün sayısı 1 ile 365 arasında olmalıdır");
            return ValidationProblem(ModelState);
        }

        var chart = await _adminService.GetRevenueChartAsync(days, cancellationToken);
        return Ok(chart);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("dashboard/top-products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(IEnumerable<AdminTopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AdminTopProductDto>>> GetTopProducts(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count > 100) count = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (count < 1) count = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var topProducts = await _adminService.GetTopProductsAsync(count, cancellationToken);
        return Ok(topProducts);
    }

    /// <summary>
    /// Envanter özetini getirir
    /// </summary>
    [HttpGet("dashboard/inventory-overview")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(InventoryOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryOverviewDto>> GetInventoryOverview(
        CancellationToken cancellationToken = default)
    {
        var overview = await _adminService.GetInventoryOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Son siparişleri getirir
    /// </summary>
    [HttpGet("orders/recent")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRecentOrders(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count > 100) count = 100; // ✅ BOLUM 3.4: Max limit kontrolü
        if (count < 1) count = 1; // ✅ BOLUM 4.1: Min limit kontrolü

        var orders = await _adminService.GetRecentOrdersAsync(count, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    [HttpGet("products/low-stock")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 4.1: Input Validation - threshold pozitif olmalı
        if (threshold < 0)
        {
            ModelState.AddModelError(nameof(threshold), "Eşik değeri 0 veya daha büyük olmalıdır");
            return ValidationProblem(ModelState);
        }

        var products = await _adminService.GetLowStockProductsAsync(threshold, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Bekleyen yorumları getirir (pagination ile)
    /// </summary>
    [HttpGet("reviews/pending")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetPendingReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var reviews = await _adminService.GetPendingReviewsAsync(page, pageSize, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Bekleyen iade taleplerini getirir (pagination ile)
    /// </summary>
    [HttpGet("returns/pending")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(PagedResult<ReturnRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReturnRequestDto>>> GetPendingReturns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var returns = await _adminService.GetPendingReturnsAsync(page, pageSize, cancellationToken);
        return Ok(returns);
    }

    /// <summary>
    /// Kullanıcıları listeler (pagination ile)
    /// </summary>
    [HttpGet("users")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var users = await _adminService.GetUsersAsync(page, pageSize, role, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Kullanıcıyı aktifleştirir
    /// </summary>
    [HttpPost("users/{userId}/activate")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.ActivateUserAsync(userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    /// <summary>
    /// Kullanıcıyı pasifleştirir
    /// </summary>
    [HttpPost("users/{userId}/deactivate")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.DeactivateUserAsync(userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    /// <summary>
    /// Kullanıcı rolünü değiştirir
    /// </summary>
    [HttpPost("users/{userId}/change-role")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangeUserRole(
        Guid userId,
        [FromBody] ChangeRoleDto roleDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _adminService.ChangeUserRoleAsync(userId, roleDto.Role, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { message = $"User role changed to {roleDto.Role}" });
    }

    /// <summary>
    /// Kullanıcıyı siler
    /// </summary>
    [HttpDelete("users/{userId}")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.DeleteUserAsync(userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    /// <summary>
    /// Analytics özetini getirir
    /// </summary>
    [HttpGet("analytics/summary")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetAnalyticsSummary(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 4.1: Input Validation - days 1-365 arasında olmalı
        if (days < 1 || days > 365)
        {
            ModelState.AddModelError(nameof(days), "Gün sayısı 1 ile 365 arasında olmalıdır");
            return ValidationProblem(ModelState);
        }

        var summary = await _adminService.GetAnalyticsSummaryAsync(days, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// 2FA istatistiklerini getirir
    /// </summary>
    [HttpGet("security/2fa-stats")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(TwoFactorStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorStatsDto>> Get2FAStats(
        CancellationToken cancellationToken = default)
    {
        var stats = await _adminService.Get2FAStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Sistem sağlık durumunu getirir
    /// </summary>
    [HttpGet("system/health")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(SystemHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth(
        CancellationToken cancellationToken = default)
    {
        var health = await _adminService.GetSystemHealthAsync(cancellationToken);
        return Ok(health);
    }
}
