using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Review;
using Merge.Application.DTOs.Review;

namespace Merge.API.Controllers.Review;

[ApiController]
[Route("api/reviews/helpfulness")]
public class ReviewHelpfulnessController : BaseController
{
    private readonly IReviewHelpfulnessService _reviewHelpfulnessService;

    public ReviewHelpfulnessController(IReviewHelpfulnessService reviewHelpfulnessService)
    {
        _reviewHelpfulnessService = reviewHelpfulnessService;
    }

    [HttpPost("mark")]
    [Authorize]
    public async Task<IActionResult> MarkReviewHelpfulness([FromBody] MarkReviewHelpfulnessDto dto)
    {
        var userId = GetUserId();
        await _reviewHelpfulnessService.MarkReviewHelpfulnessAsync(userId, dto);
        return Ok();
    }

    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<IActionResult> RemoveHelpfulnessVote(Guid reviewId)
    {
        var userId = GetUserId();
        await _reviewHelpfulnessService.RemoveHelpfulnessVoteAsync(userId, reviewId);
        return Ok();
    }

    [HttpGet("stats/{reviewId}")]
    public async Task<ActionResult<ReviewHelpfulnessStatsDto>> GetReviewHelpfulnessStats(Guid reviewId)
    {
        var userId = GetUserIdOrNull();
        var stats = await _reviewHelpfulnessService.GetReviewHelpfulnessStatsAsync(reviewId, userId);
        return Ok(stats);
    }

    [HttpGet("most-helpful/{productId}")]
    public async Task<ActionResult<IEnumerable<ReviewHelpfulnessStatsDto>>> GetMostHelpfulReviews(Guid productId, [FromQuery] int limit = 10)
    {
        var reviews = await _reviewHelpfulnessService.GetMostHelpfulReviewsAsync(productId, limit);
        return Ok(reviews);
    }
}
