using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/search/recommendations")]
public class RecommendationsController : BaseController
{
    private readonly IProductRecommendationService _recommendationService;

    public RecommendationsController(IProductRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("similar/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetSimilarProducts(
        Guid productId,
        [FromQuery] int maxResults = 10)
    {
        var recommendations = await _recommendationService.GetSimilarProductsAsync(productId, maxResults);
        return Ok(recommendations);
    }

    [HttpGet("frequently-bought-together/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetFrequentlyBoughtTogether(
        Guid productId,
        [FromQuery] int maxResults = 5)
    {
        var recommendations = await _recommendationService.GetFrequentlyBoughtTogetherAsync(productId, maxResults);
        return Ok(recommendations);
    }

    [HttpGet("for-you")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetPersonalizedRecommendations(
        [FromQuery] int maxResults = 10)
    {
        var userId = GetUserId();
        var recommendations = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, maxResults);
        return Ok(recommendations);
    }

    [HttpGet("based-on-history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetBasedOnHistory(
        [FromQuery] int maxResults = 10)
    {
        var userId = GetUserId();
        var recommendations = await _recommendationService.GetBasedOnViewHistoryAsync(userId, maxResults);
        return Ok(recommendations);
    }

    [HttpGet("trending")]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetTrendingProducts(
        [FromQuery] int days = 7,
        [FromQuery] int maxResults = 10)
    {
        var recommendations = await _recommendationService.GetTrendingProductsAsync(days, maxResults);
        return Ok(recommendations);
    }

    [HttpGet("best-sellers")]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetBestSellers(
        [FromQuery] int maxResults = 10)
    {
        var recommendations = await _recommendationService.GetBestSellersAsync(maxResults);
        return Ok(recommendations);
    }

    [HttpGet("new-arrivals")]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetNewArrivals(
        [FromQuery] int days = 30,
        [FromQuery] int maxResults = 10)
    {
        var recommendations = await _recommendationService.GetNewArrivalsAsync(days, maxResults);
        return Ok(recommendations);
    }

    [HttpGet("complete")]
    [Authorize]
    public async Task<ActionResult<PersonalizedRecommendationsDto>> GetCompleteRecommendations()
    {
        var userId = GetUserId();
        var recommendations = await _recommendationService.GetCompleteRecommendationsAsync(userId);
        return Ok(recommendations);
    }
}
