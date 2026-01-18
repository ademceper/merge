using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetInventoryAnalytics;
using Merge.Application.Analytics.Queries.GetLowStockProducts;
using Merge.Application.Analytics.Queries.GetStockByWarehouse;

namespace Merge.API.Controllers.Analytics.Inventory;

/// <summary>
/// Inventory Analytics API endpoints.
/// Envanter analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/inventory")]
[Authorize(Roles = "Admin,Manager")]
[Tags("InventoryAnalytics")]
public class InventoryAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Envanter analitiklerini getirir
    /// </summary>
    [HttpGet("inventory")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(InventoryAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryAnalyticsDto>> GetInventoryAnalytics(
        CancellationToken cancellationToken = default)
    {
        var query = new GetInventoryAnalyticsQuery();
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    [HttpGet("inventory/low-stock")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<LowStockProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<LowStockProductDto>>> GetLowStockProducts(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLowStockProductsQuery(threshold);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Depo bazında stok bilgilerini getirir
    /// </summary>
    [HttpGet("inventory/by-warehouse")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<WarehouseStockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<WarehouseStockDto>>> GetStockByWarehouse(
        CancellationToken cancellationToken = default)
    {
        var query = new GetStockByWarehouseQuery();
        var stock = await mediator.Send(query, cancellationToken);
        return Ok(stock);
    }

    
}
