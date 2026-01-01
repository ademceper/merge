using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Review;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Review;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : BaseController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetByProduct(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        var reviews = await _reviewService.GetByProductIdAsync(productId, page, pageSize);
        return Ok(reviews);
    }

    // ✅ PERFORMANCE: Pagination ekle (BEST_PRACTICES_ANALIZI.md - BOLUM 3.1.4)
    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        var userId = GetUserId();
        var reviews = await _reviewService.GetByUserIdAsync(userId, page, pageSize);
        return Ok(reviews);
    }

    // ✅ SECURITY: Rate limiting - 5 yorum oluşturma / saat (spam koruması)
    [HttpPost]
    [Authorize]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        dto.UserId = userId;
        var review = await _reviewService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetByProduct), new { productId = review.ProductId }, review);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReviewDto>> Update(Guid id, [FromBody] UpdateReviewDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ SECURITY: Authorization check - kullanıcı sadece kendi review'lerini güncelleyebilmeli
        var userId = GetUserId();
        var existingReview = await _reviewService.GetByIdAsync(id);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var review = await _reviewService.UpdateAsync(id, dto);
        if (review == null)
        {
            return NotFound();
        }
        return Ok(review);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // ✅ SECURITY: Authorization check - kullanıcı sadece kendi review'lerini silebilmeli
        var userId = GetUserId();
        var existingReview = await _reviewService.GetByIdAsync(id);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _reviewService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _reviewService.ApproveReviewAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReviewDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _reviewService.RejectReviewAsync(id, dto.Reason);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

}

