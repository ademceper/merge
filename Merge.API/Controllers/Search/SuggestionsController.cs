using MediatR;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Search;
using Merge.Application.Search.Queries.GetAutocompleteSuggestions;
using Merge.Application.Search.Queries.GetPopularSearches;
using Merge.Application.Search.Queries.GetTrendingSearches;
using Merge.Application.Search.Commands.RecordSearch;
using Merge.Application.Search.Commands.RecordClick;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/v{version:apiVersion}/search/suggestions")]
public class SearchSuggestionsController : BaseController
{
    private readonly IMediator _mediator;

    public SearchSuggestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Arama önerileri (autocomplete) getirir
    /// </summary>
    /// <param name="q">Arama terimi (minimum 2 karakter)</param>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Autocomplete önerileri (ürünler ve kategoriler)</returns>
    /// <response code="200">Öneriler başarıyla getirildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("autocomplete")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(AutocompleteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AutocompleteResultDto>> GetAutocomplete(
        [FromQuery] string q,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetAutocompleteSuggestionsQuery(q, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Popüler arama terimlerini getirir
    /// </summary>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Popüler arama terimleri listesi</returns>
    /// <response code="200">Popüler aramalar başarıyla getirildi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("popular")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<string>>> GetPopularSearches(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetPopularSearchesQuery(maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Trend olan arama terimlerini getirir
    /// </summary>
    /// <param name="days">Son kaç günün trend verileri alınacak (varsayılan: 7)</param>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Trend arama terimleri listesi</returns>
    /// <response code="200">Trend aramalar başarıyla getirildi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("trending")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<SearchSuggestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SearchSuggestionDto>>> GetTrendingSearches(
        [FromQuery] int days = 7,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetTrendingSearchesQuery(days, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Arama işlemini kaydeder (analytics için)
    /// </summary>
    /// <param name="recordDto">Arama kayıt verisi (arama terimi, sonuç sayısı)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı kayıt (204 No Content)</returns>
    /// <response code="204">Arama başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("record")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (Spam koruması)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordSearch(
        [FromBody] RecordSearchDto recordDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
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

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Arama sonucu tıklama işlemini kaydeder (analytics için)
    /// </summary>
    /// <param name="clickDto">Tıklama kayıt verisi (arama geçmişi ID, ürün ID)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı kayıt (204 No Content)</returns>
    /// <response code="204">Tıklama başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("record-click")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (Spam koruması)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RecordClick(
        [FromBody] RecordClickDto clickDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new RecordClickCommand(clickDto.SearchHistoryId, clickDto.ProductId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
