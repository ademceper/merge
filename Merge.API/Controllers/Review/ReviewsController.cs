using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.Review.Commands.CreateReview;
using Merge.Application.Review.Commands.UpdateReview;
using Merge.Application.Review.Commands.DeleteReview;
using Merge.Application.Review.Commands.ApproveReview;
using Merge.Application.Review.Commands.RejectReview;
using Merge.Application.Review.Queries.GetReviewById;
using Merge.Application.Review.Queries.GetReviewsByProductId;
using Merge.Application.Review.Queries.GetReviewsByUserId;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Review;

/// <summary>
/// Reviews Controller - Review management endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews")]
public class ReviewsController : BaseController
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get reviews by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of reviews</returns>
    /// <response code="200">Returns the paged list of reviews</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var query = new GetReviewsByProductIdQuery(productId, page, pageSize);
        var reviews = await _mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Get current user's reviews
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of user's reviews</returns>
    /// <response code="200">Returns the paged list of reviews</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("user")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var userId = GetUserId();
        var query = new GetReviewsByUserIdQuery(userId, page, pageSize);
        var reviews = await _mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Create a new review
    /// </summary>
    /// <param name="dto">Review creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review</returns>
    /// <response code="201">Review created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Too many requests</response>
    [HttpPost]
    [Authorize]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5/saat (Spam koruması)
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewDto>> Create(
        [FromBody] CreateReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var userId = GetUserId();
        var command = new CreateReviewCommand(
            userId,
            dto.ProductId,
            dto.Rating,
            dto.Title,
            dto.Comment);

        var review = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByProduct), new { productId = review.ProductId }, review);
    }

    /// <summary>
    /// Update a review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="dto">Review update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated review</returns>
    /// <response code="200">Review updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - User can only update their own reviews</response>
    /// <response code="404">Review not found</response>
    /// <response code="429">Too many requests</response>
    [HttpPut("{id}")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewDto>> Update(
        Guid id,
        [FromBody] UpdateReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi review'lerini güncelleyebilmeli
        var userId = GetUserId();
        var existingReviewQuery = new GetReviewByIdQuery(id);
        var existingReview = await _mediator.Send(existingReviewQuery, cancellationToken);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        // ✅ SECURITY: IDOR koruması - UserId command'a ekleniyor
        var command = new UpdateReviewCommand(id, userId, dto.Rating, dto.Title, dto.Comment);
        var review = await _mediator.Send(command, cancellationToken);
        return Ok(review);
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Review deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - User can only delete their own reviews</response>
    /// <response code="404">Review not found</response>
    /// <response code="429">Too many requests</response>
    [HttpDelete("{id}")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi review'lerini silebilmeli
        var userId = GetUserId();
        var existingReviewQuery = new GetReviewByIdQuery(id);
        var existingReview = await _mediator.Send(existingReviewQuery, cancellationToken);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        // ✅ SECURITY: IDOR koruması - UserId command'a ekleniyor
        var command = new DeleteReviewCommand(id, userId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Approve a review (Admin only)
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Review approved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Review not found</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        // ✅ SECURITY: Audit için ApprovedByUserId eklendi
        var approvedByUserId = GetUserId();
        var command = new ApproveReviewCommand(id, approvedByUserId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Reject a review (Admin only)
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="dto">Rejection reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Review rejected successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Review not found</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectReviewDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        // ✅ SECURITY: Audit için RejectedByUserId eklendi
        var rejectedByUserId = GetUserId();
        var command = new RejectReviewCommand(id, rejectedByUserId, dto.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

}

