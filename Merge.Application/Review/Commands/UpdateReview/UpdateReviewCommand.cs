using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Commands.UpdateReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ SECURITY: IDOR koruması için UserId eklendi
public record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId, // IDOR protection
    int Rating,
    string Title,
    string Comment
) : IRequest<ReviewDto>;
