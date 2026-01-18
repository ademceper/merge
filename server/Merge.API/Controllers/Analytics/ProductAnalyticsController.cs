using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetProductAnalytics;
using Merge.Application.Analytics.Queries.GetWorstPerformers;
using Merge.Application.Analytics.Queries.GetBestSellers;

namespace Merge.API.Controllers.Analytics.Product;

/// <summary>
/// Product Analytics API endpoints.
/// Ürün analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/products")]
[Authorize(Roles = "Admin,Manager")]
[Tags("ProductAnalytics")]
public class ProductAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Ürün analitiklerini getirir
    /// </summary>
    [HttpGet("products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(ProductAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductAnalyticsDto>> GetProductAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("products/best-sellers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopProductDto>>> GetBestSellers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        var query = new GetBestSellersQuery(limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// En az performans gösteren ürünleri getirir
    /// </summary>
    [HttpGet("products/worst-performers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopProductDto>>> GetWorstPerformers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        var query = new GetWorstPerformersQuery(limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    
}
