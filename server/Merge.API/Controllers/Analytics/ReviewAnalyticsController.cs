using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetReviewAnalytics;
using Merge.Application.Analytics.Queries.GetRatingDistribution;
using Merge.Application.Analytics.Queries.GetReviewTrends;
using Merge.Application.Analytics.Queries.GetTopReviewedProducts;
using Merge.Application.Analytics.Queries.GetTopReviewers;

namespace Merge.API.Controllers.Analytics.Review;

/// <summary>
/// Review Analytics API endpoints.
/// Yorum analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/reviews")]
[Authorize(Roles = "Admin,Manager")]
[Tags("ReviewAnalytics")]
public class ReviewAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Yorum analitiklerini getirir
    /// </summary>
    [HttpGet("reviews")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(ReviewAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewAnalyticsDto>> GetReviewAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Puan dağılımını getirir
    /// </summary>
    [HttpGet("reviews/rating-distribution")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<RatingDistributionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<RatingDistributionDto>>> GetRatingDistribution(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRatingDistributionQuery(startDate, endDate);
        var distribution = await mediator.Send(query, cancellationToken);
        return Ok(distribution);
    }

    /// <summary>
    /// Yorum trendlerini getirir
    /// </summary>
    [HttpGet("reviews/trends")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<ReviewTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<ReviewTrendDto>>> GetReviewTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewTrendsQuery(startDate, endDate);
        var trends = await mediator.Send(query, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// En çok yorumlanan ürünleri getirir
    /// </summary>
    [HttpGet("reviews/top-products")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopReviewedProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopReviewedProductDto>>> GetTopReviewedProducts(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        var query = new GetTopReviewedProductsQuery(limit);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// En aktif yorumcuları getirir
    /// </summary>
    [HttpGet("reviews/top-reviewers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<ReviewerStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<ReviewerStatsDto>>> GetTopReviewers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        var query = new GetTopReviewersQuery(limit);
        var reviewers = await mediator.Send(query, cancellationToken);
        return Ok(reviewers);
    }

    
}
