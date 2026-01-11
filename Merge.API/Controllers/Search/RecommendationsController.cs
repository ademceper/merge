using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Product;
using Merge.Application.Search.Queries.GetSimilarProducts;
using Merge.Application.Search.Queries.GetFrequentlyBoughtTogether;
using Merge.Application.Search.Queries.GetPersonalizedRecommendations;
using Merge.Application.Search.Queries.GetBasedOnViewHistory;
using Merge.Application.Search.Queries.GetTrendingProducts;
using Merge.Application.Search.Queries.GetBestSellers;
using Merge.Application.Search.Queries.GetNewArrivals;
using Merge.Application.Search.Queries.GetCompleteRecommendations;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/v{version:apiVersion}/search/recommendations")]
public class RecommendationsController : BaseController
{
    private readonly IMediator _mediator;

    public RecommendationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Belirtilen ürüne benzer ürünleri getirir
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Benzer ürünler listesi</returns>
    /// <response code="200">Benzer ürünler başarıyla getirildi</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("similar/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetSimilarProducts(
        Guid productId,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetSimilarProductsQuery(productId, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen ürünle birlikte sık satın alınan ürünleri getirir
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 5)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Birlikte satın alınan ürünler listesi</returns>
    /// <response code="200">Birlikte satın alınan ürünler başarıyla getirildi</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("frequently-bought-together/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetFrequentlyBoughtTogether(
        Guid productId,
        [FromQuery] int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetFrequentlyBoughtTogetherQuery(productId, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcı için kişiselleştirilmiş ürün önerileri getirir (authentication gerekli)
    /// </summary>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kişiselleştirilmiş ürün önerileri listesi</returns>
    /// <response code="200">Kişiselleştirilmiş öneriler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("for-you")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetPersonalizedRecommendations(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        var query = new GetPersonalizedRecommendationsQuery(userId, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcının görüntüleme geçmişine göre ürün önerileri getirir (authentication gerekli)
    /// </summary>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Görüntüleme geçmişine göre ürün önerileri listesi</returns>
    /// <response code="200">Geçmişe göre öneriler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("based-on-history")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetBasedOnHistory(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        var query = new GetBasedOnViewHistoryQuery(userId, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Trend olan ürünleri getirir
    /// </summary>
    /// <param name="days">Son kaç günün trend verileri alınacak (varsayılan: 7)</param>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Trend ürünler listesi</returns>
    /// <response code="200">Trend ürünler başarıyla getirildi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("trending")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetTrendingProducts(
        [FromQuery] int days = 7,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetTrendingProductsQuery(days, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>En çok satan ürünler listesi</returns>
    /// <response code="200">En çok satan ürünler başarıyla getirildi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("best-sellers")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetBestSellers(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetBestSellersQuery(maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni eklenen ürünleri getirir
    /// </summary>
    /// <param name="days">Son kaç gün içinde eklenen ürünler (varsayılan: 30)</param>
    /// <param name="maxResults">Maksimum sonuç sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yeni eklenen ürünler listesi</returns>
    /// <response code="200">Yeni ürünler başarıyla getirildi</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("new-arrivals")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetNewArrivals(
        [FromQuery] int days = 30,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetNewArrivalsQuery(days, maxResults);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcı için tüm öneri kategorilerini getirir (authentication gerekli)
    /// Kişiselleştirilmiş, geçmişe göre, trend ve en çok satan ürünleri içerir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tüm öneri kategorileri (ForYou, BasedOnHistory, Trending, BestSellers)</returns>
    /// <response code="200">Tüm öneriler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Rate limit aşıldı</response>
    // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("complete")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PersonalizedRecommendationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PersonalizedRecommendationsDto>> GetCompleteRecommendations(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        var query = new GetCompleteRecommendationsQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
