using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.Review.Commands.MarkReviewHelpfulness;
using Merge.Application.Review.Commands.RemoveReviewHelpfulness;
using Merge.Application.Review.Queries.GetReviewHelpfulnessStats;
using Merge.Application.Review.Queries.GetMostHelpfulReviews;
using Merge.Application.DTOs.Review;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Review;

/// <summary>
/// Review Helpfulness Controller - Review helpfulness voting endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews/helpfulness")]
public class ReviewHelpfulnessController : BaseController
{
    private readonly IMediator _mediator;

    public ReviewHelpfulnessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Mark review as helpful or not helpful
    /// </summary>
    /// <param name="dto">Helpfulness vote data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Helpfulness vote recorded successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("mark")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkReviewHelpfulness(
        [FromBody] MarkReviewHelpfulnessDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var userId = GetUserId();
        var command = new MarkReviewHelpfulnessCommand(userId, dto.ReviewId, dto.IsHelpful);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Remove helpfulness vote
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Helpfulness vote removed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Too many requests</response>
    [HttpDelete("{reviewId}")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveHelpfulnessVote(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var userId = GetUserId();
        var command = new RemoveReviewHelpfulnessCommand(userId, reviewId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get review helpfulness statistics
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Review helpfulness statistics</returns>
    /// <response code="200">Returns review helpfulness statistics</response>
    /// <response code="404">Review not found</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("stats/{reviewId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ReviewHelpfulnessStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewHelpfulnessStatsDto>> GetReviewHelpfulnessStats(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var userId = GetUserIdOrNull();
        var query = new GetReviewHelpfulnessStatsQuery(reviewId, userId);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get most helpful reviews for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="limit">Maximum number of reviews to return (default: 10, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of most helpful reviews</returns>
    /// <response code="200">Returns list of most helpful reviews</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("most-helpful/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ReviewHelpfulnessStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ReviewHelpfulnessStatsDto>>> GetMostHelpfulReviews(
        Guid productId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var query = new GetMostHelpfulReviewsQuery(productId, limit);
        var reviews = await _mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }
}
