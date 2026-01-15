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

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seller/dashboard")]
[Authorize(Roles = "Seller,Admin")]
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStats", new { version }, version);
        links["orders"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/orders", Method = "GET" };
        links["products"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/products", Method = "GET" };
        links["performance"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance", Method = "GET" };
        return Ok(new { stats, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetOrders", page, pageSize, result.TotalPages, new { version }, version);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetProducts", page, pageSize, result.TotalPages, new { version }, version);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetPerformance", new { version, startDate, endDate }, version);
        links["detailed"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance/detailed?startDate={startDate}&endDate={endDate}", Method = "GET" };
        links["categories"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance/categories?startDate={startDate}&endDate={endDate}", Method = "GET" };
        return Ok(new { performance, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetDetailedPerformance", new { version, startDate, endDate }, version);
        links["performance"] = new LinkDto { Href = $"/api/v{version}/seller/dashboard/performance?startDate={startDate}&endDate={endDate}", Method = "GET" };
        return Ok(new { performance, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetCategoryPerformance", new { version, startDate, endDate }, version);
        return Ok(new { performance, _links = links });
    }
}
