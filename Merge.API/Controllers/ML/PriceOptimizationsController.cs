using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Services;
using Merge.Application.Interfaces.ML;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiController]
[Route("api/ml/price-optimization")]
[Authorize(Roles = "Admin,Manager")]
public class PriceOptimizationsController : BaseController
{
    private readonly IPriceOptimizationService _priceOptimizationService;

    public PriceOptimizationsController(IPriceOptimizationService priceOptimizationService)
    {
        _priceOptimizationService = priceOptimizationService;
    }

    /// <summary>
    /// Ürün için fiyat optimizasyonu yapar (Admin, Manager)
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _priceOptimizationService.OptimizePriceAsync(productId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kategori için fiyat optimizasyonu yapar (pagination ile) (Admin, Manager)
    /// </summary>
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
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit
        if (page < 1) page = 1;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var allResults = await _priceOptimizationService.OptimizePricesForCategoryAsync(categoryId, request, cancellationToken);
        var resultsList = allResults.ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = resultsList.Count;
        var pagedResults = resultsList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<PriceOptimizationDto>
        {
            Items = pagedResults,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Ürün için fiyat önerisi getirir (Admin, Manager)
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var recommendation = await _priceOptimizationService.GetPriceRecommendationAsync(productId, cancellationToken);
        return Ok(recommendation);
    }

    /// <summary>
    /// Fiyat optimizasyon istatistiklerini getirir (Admin, Manager)
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _priceOptimizationService.GetOptimizationStatsAsync(startDate, endDate, cancellationToken);
        return Ok(stats);
    }
}
