using MediatR;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Search;
using Merge.Application.Search.Queries.SearchProducts;
using Merge.API.Middleware;
using Merge.API.Helpers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/v{version:apiVersion}/search")]
public class SearchController : BaseController
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ürün arama işlemi yapar
    /// </summary>
    /// <param name="request">Arama isteği (arama terimi, kategori, marka, fiyat aralığı, vb.)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Arama sonuçları (ürünler, toplam sayı, sayfalama bilgileri, HATEOAS link'leri)</returns>
    /// <response code="200">Arama başarılı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromBody] SearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
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

        var result = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1: HATEOAS (ZORUNLU)
        return Ok(HateoasHelper.AddSearchLinks(result, Request));
    }

    /// <summary>
    /// Hızlı ürün arama işlemi yapar (query string parametreleri ile)
    /// </summary>
    /// <param name="q">Arama terimi</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Arama sonuçları (ürünler, toplam sayı, sayfalama bilgileri, HATEOAS link'leri)</returns>
    /// <response code="200">Arama başarılı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("quick")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SearchResultDto>> QuickSearch(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new SearchProductsQuery(
            SearchTerm: q,
            Page: page < 1 ? 1 : page,
            PageSize: pageSize > 100 ? 100 : pageSize
        );

        var result = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1: HATEOAS (ZORUNLU)
        return Ok(HateoasHelper.AddSearchLinks(result, Request));
    }
}

