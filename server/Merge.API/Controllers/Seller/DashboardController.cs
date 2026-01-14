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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Seller;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seller/dashboard")]
[Authorize(Roles = "Seller,Admin")]
public class DashboardController : BaseController
{
    private readonly IMediator _mediator;
    
    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Dashboard istatistiklerini getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SellerDashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerDashboardStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetDashboardStatsQuery(sellerId);
        var stats = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStats", new { version }, version);
        links["orders"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/orders", Method = "GET" };
        links["products"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/products", Method = "GET" };
        links["performance"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance", Method = "GET" };

        return Ok(new { stats, _links = links });
    }

    /// <summary>
    /// Satıcının siparişlerini getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("orders")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<object>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSellerOrdersQuery(sellerId, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetOrders", page, pageSize, result.TotalPages, new { version }, version);

        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
    }

    /// <summary>
    /// Satıcının ürünlerini getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("products")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<object>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetSellerProductsQuery(sellerId, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetProducts", page, pageSize, result.TotalPages, new { version }, version);

        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
    }

    /// <summary>
    /// Performans metriklerini getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("performance")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SellerPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerPerformanceDto>> GetPerformance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPerformanceMetricsQuery(sellerId, startDate, endDate);
        var performance = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetPerformance", new { version, startDate, endDate }, version);
        links["detailed"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance/detailed?startDate={startDate}&endDate={endDate}", Method = "GET" };
        links["categories"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance/categories?startDate={startDate}&endDate={endDate}", Method = "GET" };

        return Ok(new { performance, _links = links });
    }

    /// <summary>
    /// Detaylı performans metriklerini getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("performance/detailed")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetDetailedPerformanceMetricsQuery(sellerId, startDate, endDate);
        var performance = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetDetailedPerformance", new { version, startDate, endDate }, version);
        links["performance"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance?startDate={startDate}&endDate={endDate}", Method = "GET" };

        return Ok(new { performance, _links = links });
    }

    /// <summary>
    /// Kategori performansını getirir
    /// </summary>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("performance/categories")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(List<CategoryPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CategoryPerformanceDto>>> GetCategoryPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sellerId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetCategoryPerformanceQuery(sellerId, startDate, endDate);
        var performance = await _mediator.Send(query, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetCategoryPerformance", new { version, startDate, endDate }, version);

        return Ok(new { performance, _links = links });
    }
}
