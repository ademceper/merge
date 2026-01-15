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

namespace Merge.API.Controllers.Search;

[ApiController]
[Route("api/v{version:apiVersion}/search/recommendations")]
public class RecommendationsController(IMediator mediator) : BaseController
{
    [HttpGet("similar/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetSimilarProducts(
        Guid productId,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSimilarProductsQuery(productId, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("frequently-bought-together/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetFrequentlyBoughtTogether(
        Guid productId,
        [FromQuery] int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFrequentlyBoughtTogetherQuery(productId, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("for-you")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetPersonalizedRecommendations(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetPersonalizedRecommendationsQuery(userId, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("based-on-history")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetBasedOnHistory(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetBasedOnViewHistoryQuery(userId, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("trending")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetTrendingProducts(
        [FromQuery] int days = 7,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTrendingProductsQuery(days, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("best-sellers")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetBestSellers(
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBestSellersQuery(maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("new-arrivals")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductRecommendationDto>>> GetNewArrivals(
        [FromQuery] int days = 30,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNewArrivalsQuery(days, maxResults);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("complete")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PersonalizedRecommendationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PersonalizedRecommendationsDto>> GetCompleteRecommendations(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetCompleteRecommendationsQuery(userId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
