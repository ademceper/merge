using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.Review.Commands.CreateReview;
using Merge.Application.Review.Commands.UpdateReview;
using Merge.Application.Review.Commands.PatchReview;
using Merge.Application.Review.Commands.DeleteReview;
using Merge.Application.Review.Commands.ApproveReview;
using Merge.Application.Review.Commands.RejectReview;
using Merge.Application.Review.Queries.GetReviewById;
using Merge.Application.Review.Queries.GetReviewsByProductId;
using Merge.Application.Review.Queries.GetReviewsByUserId;
using Merge.Application.DTOs.Review;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Review;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews")]
public class ReviewsController(IMediator mediator) : BaseController
{
    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReviewsByProductIdQuery(productId, page, pageSize);
        var reviews = await mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("user")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetReviewsByUserIdQuery(userId, page, pageSize);
        var reviews = await mediator.Send(query, cancellationToken);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    [RateLimit(5, 3600)]
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
        var userId = GetUserId();
        var command = new CreateReviewCommand(
            userId,
            dto.ProductId,
            dto.Rating,
            dto.Title,
            dto.Comment);
        var review = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByProduct), new { productId = review.ProductId }, review);
    }

    [HttpPut("{id}")]
    [Authorize]
    [RateLimit(30, 60)]
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
        var userId = GetUserId();
        var existingReviewQuery = new GetReviewByIdQuery(id);
        var existingReview = await mediator.Send(existingReviewQuery, cancellationToken);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new UpdateReviewCommand(id, userId, dto.Rating, dto.Title, dto.Comment);
        var review = await mediator.Send(command, cancellationToken);
        return Ok(review);
    }

    /// <summary>
    /// Değerlendirmeyi kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReviewDto>> Patch(
        Guid id,
        [FromBody] PatchReviewDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var userId = GetUserId();
        var existingReviewQuery = new GetReviewByIdQuery(id);
        var existingReview = await mediator.Send(existingReviewQuery, cancellationToken);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new PatchReviewCommand(id, userId, patchDto);
        var review = await mediator.Send(command, cancellationToken);
        return Ok(review);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var existingReviewQuery = new GetReviewByIdQuery(id);
        var existingReview = await mediator.Send(existingReviewQuery, cancellationToken);
        if (existingReview == null)
        {
            return NotFound();
        }
        if (existingReview.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new DeleteReviewCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken = default)
    {
        var approvedByUserId = GetUserId();
        var command = new ApproveReviewCommand(id, approvedByUserId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
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
        var rejectedByUserId = GetUserId();
        var command = new RejectReviewCommand(id, rejectedByUserId, dto.Reason);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

}
