using MediatR;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Search;
using Merge.Application.Search.Queries.SearchProducts;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/v{version:apiVersion}/search")]
public class SearchController(IMediator mediator) : BaseController
{
    [HttpPost]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromBody] SearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var query = new SearchProductsQuery(
            request.SearchTerm,
            request.CategoryId,
            request.Brand,
            request.MinPrice,
            request.MaxPrice,
            request.MinRating,
            request.InStockOnly,
            request.SortBy,
            request.Page ?? 1,
            request.PageSize ?? 20
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
