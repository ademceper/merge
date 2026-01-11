using MediatR;

namespace Merge.Application.Review.Commands.DeleteReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ SECURITY: IDOR koruması için UserId eklendi
public record DeleteReviewCommand(
    Guid ReviewId,
    Guid UserId // IDOR protection
) : IRequest<bool>;
