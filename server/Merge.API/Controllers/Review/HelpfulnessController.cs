using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.Review.Commands.MarkReviewHelpfulness;
using Merge.Application.Review.Commands.RemoveReviewHelpfulness;
using Merge.Application.Review.Queries.GetReviewHelpfulnessStats;
using Merge.Application.Review.Queries.GetMostHelpfulReviews;
using Merge.Application.DTOs.Review;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Review;

/// <summary>
/// Review Helpfulness API endpoints.
/// Yorum faydalılık işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/reviews/helpfulness")]
[Tags("ReviewHelpfulness")]
public class ReviewHelpfulnessController(IMediator mediator) : BaseController
{
    [HttpPost("mark")]
    [Authorize]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkReviewHelpfulness(
        [FromBody] MarkReviewHelpfulnessDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var userId = GetUserId();
        var command = new MarkReviewHelpfulnessCommand(userId, dto.ReviewId, dto.IsHelpful);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{reviewId}")]
    [Authorize]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveHelpfulnessVote(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new RemoveReviewHelpfulnessCommand(userId, reviewId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("stats/{reviewId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ReviewHelpfulnessStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewHelpfulnessStatsDto>> GetReviewHelpfulnessStats(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        var query = new GetReviewHelpfulnessStatsQuery(reviewId, userId);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("most-helpful/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ReviewHelpfulnessStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ReviewHelpfulnessStatsDto>>> GetMostHelpfulReviews(
        Guid productId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMostHelpfulReviewsQuery(productId, limit);
        var reviews = await mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }
}
