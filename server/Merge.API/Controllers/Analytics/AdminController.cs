using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;
using Merge.Application.Common;
using Merge.Application.Analytics.Queries.GetDashboardStats;
using Merge.Application.Analytics.Queries.GetRevenueChart;
using Merge.Application.Analytics.Queries.GetAdminTopProducts;
using Merge.Application.Analytics.Queries.GetInventoryOverview;
using Merge.Application.Analytics.Queries.GetRecentOrders;
using Merge.Application.Analytics.Queries.GetAdminLowStockProducts;
using Merge.Application.Analytics.Queries.GetPendingReviews;
using Merge.Application.Analytics.Queries.GetPendingReturns;
using Merge.Application.Analytics.Queries.GetUsers;
using Merge.Application.Analytics.Queries.GetAnalyticsSummary;
using Merge.Application.Analytics.Queries.Get2FAStats;
using Merge.Application.Analytics.Queries.GetSystemHealth;
using Merge.Application.Analytics.Commands.ActivateUser;
using Merge.Application.Analytics.Commands.DeactivateUser;
using Merge.Application.Analytics.Commands.ChangeUserRole;
using Merge.Application.Analytics.Commands.DeleteUser;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Analytics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    [HttpGet("dashboard/stats")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Gelir grafiğini getirir
    /// </summary>
    [HttpGet("dashboard/revenue-chart")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(RevenueChartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RevenueChartDto>> GetRevenueChart(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRevenueChartQuery(days);
        var chart = await mediator.Send(query, cancellationToken);
        return Ok(chart);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("dashboard/top-products")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(IEnumerable<AdminTopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AdminTopProductDto>>> GetTopProducts(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count > paginationSettings.Value.MaxPageSize) count = paginationSettings.Value.MaxPageSize;
        if (count < 1) count = 1;

        var query = new GetAdminTopProductsQuery(count);
        var topProducts = await mediator.Send(query, cancellationToken);
        return Ok(topProducts);
    }

    /// <summary>
    /// Envanter özetini getirir
    /// </summary>
    [HttpGet("dashboard/inventory-overview")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(InventoryOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryOverviewDto>> GetInventoryOverview(
        CancellationToken cancellationToken = default)
    {
        var query = new GetInventoryOverviewQuery();
        var overview = await mediator.Send(query, cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Son siparişleri getirir
    /// </summary>
    [HttpGet("orders/recent")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRecentOrders(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count > paginationSettings.Value.MaxPageSize) count = paginationSettings.Value.MaxPageSize;
        if (count < 1) count = 1;

        var query = new GetRecentOrdersQuery(count);
        var orders = await mediator.Send(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    [HttpGet("products/low-stock")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminLowStockProductsQuery(threshold);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Bekleyen yorumları getirir (pagination ile)
    /// </summary>
    [HttpGet("reviews/pending")]
    [RateLimit(30, 60)]
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
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetPendingReviewsQuery(page, pageSize);
        var reviews = await mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Bekleyen iade taleplerini getirir (pagination ile)
    /// </summary>
    [HttpGet("returns/pending")]
    [RateLimit(30, 60)]
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
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetPendingReturnsQuery(page, pageSize);
        var returns = await mediator.Send(query, cancellationToken);
        return Ok(returns);
    }

    /// <summary>
    /// Kullanıcıları listeler (pagination ile)
    /// </summary>
    [HttpGet("users")]
    [RateLimit(30, 60)]
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
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetUsersQuery(page, pageSize, role);
        var users = await mediator.Send(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Kullanıcıyı aktifleştirir
    /// </summary>
    [HttpPost("users/{userId}/activate")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateUserCommand(userId);
        var result = await mediator.Send(command, cancellationToken);
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
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await mediator.Send(command, cancellationToken);
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
    [RateLimit(5, 60)]
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
        var command = new ChangeUserRoleCommand(userId, roleDto.Role);
        var result = await mediator.Send(command, cancellationToken);
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
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteUserCommand(userId);
        var result = await mediator.Send(command, cancellationToken);
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
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetAnalyticsSummary(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAnalyticsSummaryQuery(days);
        var summary = await mediator.Send(query, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// 2FA istatistiklerini getirir
    /// </summary>
    [HttpGet("security/2fa-stats")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(TwoFactorStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TwoFactorStatsDto>> Get2FAStats(
        CancellationToken cancellationToken = default)
    {
        var query = new Get2FAStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Sistem sağlık durumunu getirir
    /// </summary>
    [HttpGet("system/health")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SystemHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth(
        CancellationToken cancellationToken = default)
    {
        var query = new GetSystemHealthQuery();
        var health = await mediator.Send(query, cancellationToken);
        return Ok(health);
    }
}
