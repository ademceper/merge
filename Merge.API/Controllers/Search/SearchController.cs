using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Application.DTOs.Search;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/search")]
public class SearchController : BaseController
{
    private readonly IProductSearchService _searchService;

    public SearchController(IProductSearchService searchService)
    {
        _searchService = searchService;
    }

    // ✅ SECURITY: Rate limiting - 60 arama / dakika (DoS koruması) - .cursorrules BOLUM 3.3
    [HttpPost]
    [RateLimit(60, 60)]
    public async Task<ActionResult<SearchResultDto>> Search([FromBody] SearchRequestDto request)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _searchService.SearchAsync(request);
        return Ok(result);
    }

    // ✅ SECURITY: Rate limiting - 60 arama / dakika (DoS koruması) - .cursorrules BOLUM 3.3
    [HttpGet("quick")]
    [RateLimit(60, 60)]
    public async Task<ActionResult<SearchResultDto>> QuickSearch([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var request = new SearchRequestDto
        {
            SearchTerm = q,
            Page = page,
            PageSize = pageSize
        };
        var result = await _searchService.SearchAsync(request);
        return Ok(result);
    }
}

