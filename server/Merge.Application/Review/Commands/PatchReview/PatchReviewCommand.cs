using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Commands.PatchReview;

/// <summary>
/// PATCH command for partial review updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchReviewCommand(
    Guid ReviewId,
    Guid UserId, // IDOR protection
    PatchReviewDto PatchDto
) : IRequest<ReviewDto>;
