using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetDashboardSummary;
using Merge.Application.Analytics.Queries.GetDashboardMetrics;
using Merge.Application.Analytics.Commands.RefreshDashboardMetrics;
using Merge.Application.Analytics.Queries.GetSalesAnalytics;
using Merge.Application.Analytics.Queries.GetRevenueOverTime;
using Merge.Application.Analytics.Queries.GetTopProducts;
using Merge.Application.Analytics.Queries.GetSalesByCategory;

namespace Merge.API.Controllers.Analytics.Dashboard;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/dashboard")]
[Authorize(Roles = "Admin,Manager")]
public class DashboardAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{
/// <summary>
    /// Dashboard özet bilgilerini getirir
    /// </summary>
    [HttpGet("dashboard/summary")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardSummaryQuery(startDate, endDate);
        var summary = await mediator.Send(query, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Dashboard metriklerini getirir
    /// </summary>
    [HttpGet("dashboard/metrics")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(List<DashboardMetricDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<DashboardMetricDto>>> GetDashboardMetrics(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardMetricsQuery(category);
        var metrics = await mediator.Send(query, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Dashboard metriklerini yeniler
    /// </summary>
    [HttpPost("dashboard/refresh")]
    [RateLimit(5, 300)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RefreshDashboardMetrics(CancellationToken cancellationToken = default)
    {
        var command = new RefreshDashboardMetricsCommand();
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Satış analitiklerini getirir
    /// </summary>
    [HttpGet("sales")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SalesAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Zaman içinde gelir trendini getirir
    /// </summary>
    [HttpGet("sales/revenue-over-time")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(List<TimeSeriesDataPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetRevenueOverTime(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval = "day",
        CancellationToken cancellationToken = default)
    {
        var query = new GetRevenueOverTimeQuery(startDate, endDate, interval);
        var data = await mediator.Send(query, cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("sales/top-products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Max limit kontrolü (config'den)
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetTopProductsQuery(startDate, endDate, limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Kategori bazında satışları getirir
    /// </summary>
    [HttpGet("sales/by-category")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<CategorySalesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CategorySalesDto>>> GetSalesByCategory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetSalesByCategoryQuery(startDate, endDate);
        var sales = await mediator.Send(query, cancellationToken);
        return Ok(sales);
    }
}
