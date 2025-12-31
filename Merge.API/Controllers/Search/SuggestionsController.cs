using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Application.DTOs.Search;


namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/search/suggestions")]
public class SearchSuggestionsController : BaseController
{
    private readonly ISearchSuggestionService _searchSuggestionService;

    public SearchSuggestionsController(ISearchSuggestionService searchSuggestionService)
    {
        _searchSuggestionService = searchSuggestionService;
    }

    [HttpGet("autocomplete")]
    public async Task<ActionResult<AutocompleteResultDto>> GetAutocomplete(
        [FromQuery] string q,
        [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest();
        }

        var suggestions = await _searchSuggestionService.GetAutocompleteSuggestionsAsync(q, maxResults);
        return Ok(suggestions);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<string>>> GetPopularSearches([FromQuery] int maxResults = 10)
    {
        var popularSearches = await _searchSuggestionService.GetPopularSearchesAsync(maxResults);
        return Ok(popularSearches);
    }

    [HttpGet("trending")]
    public async Task<ActionResult<IEnumerable<SearchSuggestionDto>>> GetTrendingSearches(
        [FromQuery] int days = 7,
        [FromQuery] int maxResults = 10)
    {
        var trendingSearches = await _searchSuggestionService.GetTrendingSearchesAsync(days, maxResults);
        return Ok(trendingSearches);
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordSearch([FromBody] RecordSearchDto recordDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = User.Identity?.IsAuthenticated == true
            ? GetUserIdOrNull()
            : null;

        var userAgent = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _searchSuggestionService.RecordSearchAsync(
            recordDto.SearchTerm,
            userId,
            recordDto.ResultCount,
            userAgent,
            ipAddress
        );

        return NoContent();
    }

    [HttpPost("record-click")]
    public async Task<IActionResult> RecordClick([FromBody] RecordClickDto clickDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _searchSuggestionService.RecordClickAsync(clickDto.SearchHistoryId, clickDto.ProductId);
        return NoContent();
    }
}
