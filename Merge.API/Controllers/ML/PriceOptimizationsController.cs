using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Services;
using Merge.Application.Interfaces.ML;
using Merge.Application.DTOs.Analytics;


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

    [HttpPost("products/{productId}")]
    public async Task<ActionResult<PriceOptimizationDto>> OptimizePrice(Guid productId, [FromBody] PriceOptimizationRequestDto? request = null)
    {
        var result = await _priceOptimizationService.OptimizePriceAsync(productId, request);
        return Ok(result);
    }

    [HttpPost("categories/{categoryId}")]
    public async Task<ActionResult<IEnumerable<PriceOptimizationDto>>> OptimizePricesForCategory(Guid categoryId)
    {
        var results = await _priceOptimizationService.OptimizePricesForCategoryAsync(categoryId);
        return Ok(results);
    }

    [HttpGet("products/{productId}/recommendation")]
    public async Task<ActionResult<PriceRecommendationDto>> GetPriceRecommendation(Guid productId)
    {
        var recommendation = await _priceOptimizationService.GetPriceRecommendationAsync(productId);
        return Ok(recommendation);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<PriceOptimizationStatsDto>> GetStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var stats = await _priceOptimizationService.GetOptimizationStatsAsync(startDate, endDate);
        return Ok(stats);
    }
}

