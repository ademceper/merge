using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.Application.Common;
using Merge.Application.Seller.Queries.GetDashboardStats;
using Merge.Application.Seller.Queries.GetSellerOrders;
using Merge.Application.Seller.Queries.GetSellerProducts;
using Merge.Application.Seller.Queries.GetPerformanceMetrics;
using Merge.Application.Seller.Queries.GetDetailedPerformanceMetrics;
using Merge.Application.Seller.Queries.GetCategoryPerformance;

namespace Merge.API.Controllers.Seller;

/// <summary>
/// Seller Dashboard API endpoints.
/// Satıcı dashboard istatistiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/seller/dashboard")]
[Authorize(Roles = "Seller,Admin")]
[Tags("SellerDashboard")]
public class DashboardController(IMediator mediator) : BaseController
{

    [HttpGet("stats")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerDashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerDashboardStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetDashboardStatsQuery(sellerId);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("orders")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<object>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerOrdersQuery(sellerId, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("products")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<object>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerProductsQuery(sellerId, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("performance")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerPerformanceDto>> GetPerformance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetPerformanceMetricsQuery(sellerId, startDate, endDate);
        var performance = await mediator.Send(query, cancellationToken);
        return Ok(performance);
    }

    [HttpGet("performance/detailed")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerPerformanceMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerPerformanceMetricsDto>> GetDetailedPerformance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetDetailedPerformanceMetricsQuery(sellerId, startDate, endDate);
        var performance = await mediator.Send(query, cancellationToken);
        return Ok(performance);
    }

    [HttpGet("performance/categories")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(List<CategoryPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CategoryPerformanceDto>>> GetCategoryPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetCategoryPerformanceQuery(sellerId, startDate, endDate);
        var performance = await mediator.Send(query, cancellationToken);
        return Ok(performance);
    }
}
