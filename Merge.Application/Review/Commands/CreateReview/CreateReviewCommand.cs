using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Commands.CreateReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateReviewCommand(
    Guid UserId,
    Guid ProductId,
    int Rating,
    string Title,
    string Comment
) : IRequest<ReviewDto>;
