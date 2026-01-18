using MediatR;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Search;
using Merge.Application.Search.Queries.SearchProducts;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Search;

/// <summary>
/// Search API endpoints.
/// Ürün arama işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/search")]
[Tags("Search")]
public class SearchController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Ürün arama (GET endpoint - REST best practice)
    /// HIGH-API-004: POST yerine GET kullanımı - Search operations should use GET with query params
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] decimal? minRating = null,
        [FromQuery] bool? inStockOnly = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery(
            searchTerm ?? string.Empty,
            categoryId,
            brand,
            minPrice,
            maxPrice,
            minRating,
            inStockOnly ?? false,
            sortBy,
            page < 1 ? 1 : page,
            pageSize > 100 ? 100 : pageSize
        );
        var result = await mediator.Send(query, cancellationToken);
        return Ok(HateoasHelper.AddSearchLinks(result, Request));
    }

    [HttpGet("quick")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SearchResultDto>> QuickSearch(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery(
            SearchTerm: q,
            Page: page < 1 ? 1 : page,
            PageSize: pageSize > 100 ? 100 : pageSize
        );
        var result = await mediator.Send(query, cancellationToken);
        return Ok(HateoasHelper.AddSearchLinks(result, Request));
    }
}
