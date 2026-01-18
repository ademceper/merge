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

/// <summary>
/// Price Optimization API endpoints.
/// Fiyat optimizasyonu işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ml/price-optimization")]
[Authorize(Roles = "Admin,Manager")]
[Tags("PriceOptimization")]
public class PriceOptimizationsController(IMediator mediator) : BaseController
{
    [HttpPost("products/{productId}")]
    [RateLimit(30, 60)]
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
        var command = new OptimizePriceCommand(productId, request);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("categories/{categoryId}")]
    [RateLimit(30, 60)]
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
        var query = new OptimizePricesForCategoryQuery(categoryId, request, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("products/{productId}/recommendation")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PriceRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PriceRecommendationDto>> GetPriceRecommendation(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPriceRecommendationQuery(productId);
        var recommendation = await mediator.Send(query, cancellationToken);
        return Ok(recommendation);
    }

    [HttpGet("stats")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(PriceOptimizationStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PriceOptimizationStatsDto>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPriceOptimizationStatsQuery(startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
