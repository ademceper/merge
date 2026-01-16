using MediatR;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Search;
using Merge.Application.Search.Queries.GetAutocompleteSuggestions;
using Merge.Application.Search.Queries.GetPopularSearches;
using Merge.Application.Search.Queries.GetTrendingSearches;
using Merge.Application.Search.Commands.RecordSearch;
using Merge.Application.Search.Commands.RecordClick;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Search;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/search/suggestions")]
public class SearchSuggestionsController(IMediator mediator) : BaseController
{
    [HttpGet("autocomplete")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(AutocompleteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AutocompleteResultDto>> GetAutocomplete(
        [FromQuery] string q,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAutocompleteSuggestionsQuery(q, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("popular")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<string>>> GetPopularSearches(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPopularSearchesQuery(maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("trending")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<SearchSuggestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SearchSuggestionDto>>> GetTrendingSearches(
        [FromQuery] int days = 7,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTrendingSearchesQuery(days, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("record")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordSearch(
        [FromBody] RecordSearchDto recordDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var userId = User.Identity?.IsAuthenticated == true
            ? GetUserIdOrNull()
            : null;
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RecordSearchCommand(
            recordDto.SearchTerm,
            userId,
            recordDto.ResultCount,
            userAgent,
            ipAddress
        );
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("record-click")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordClick(
        [FromBody] RecordClickDto clickDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new RecordClickCommand(clickDto.SearchHistoryId, clickDto.ProductId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
