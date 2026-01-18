using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Review.Commands.UpdateReview;

public record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId, // IDOR protection
    int Rating,
    string Title,
    string Comment
) : IRequest<ReviewDto>;
