using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.Application.ML.Commands.OptimizePrice;
using Merge.Application.ML.Queries.OptimizePricesForCategory;
using Merge.Application.ML.Queries.GetPriceRecommendation;
using Merge.Application.ML.Queries.GetPriceOptimizationStats;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ml/price-optimization")]
[Authorize(Roles = "Admin,Manager")]
public class PriceOptimizationsController : BaseController
{
    private readonly IMediator _mediator;

    public PriceOptimizationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Ürün için fiyat optimizasyonu yapar (Admin, Manager)
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="request">Fiyat optimizasyon isteği (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fiyat optimizasyon sonuçları</returns>
    [HttpPost("products/{productId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PriceOptimizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PriceOptimizationDto>> OptimizePrice(
        Guid productId,
        [FromBody] PriceOptimizationRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new OptimizePriceCommand(productId, request);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kategori için fiyat optimizasyonu yapar (pagination ile) (Admin, Manager)
    /// </summary>
    /// <param name="categoryId">Kategori ID</param>
    /// <param name="request">Fiyat optimizasyon isteği (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış fiyat optimizasyon sonuçları</returns>
    [HttpPost("categories/{categoryId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PagedResult<PriceOptimizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PriceOptimizationDto>>> OptimizePricesForCategory(
        Guid categoryId,
        [FromBody] PriceOptimizationRequestDto? request = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new OptimizePricesForCategoryQuery(categoryId, request, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ürün için fiyat önerisi getirir (Admin, Manager)
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fiyat önerisi</returns>
    [HttpGet("products/{productId}/recommendation")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PriceRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PriceRecommendationDto>> GetPriceRecommendation(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPriceRecommendationQuery(productId);
        var recommendation = await _mediator.Send(query, cancellationToken);
        return Ok(recommendation);
    }

    /// <summary>
    /// Fiyat optimizasyon istatistiklerini getirir (Admin, Manager)
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Fiyat optimizasyon istatistikleri</returns>
    [HttpGet("stats")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PriceOptimizationStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PriceOptimizationStatsDto>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPriceOptimizationStatsQuery(startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
