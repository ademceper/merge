using MediatR;

namespace Merge.Application.Review.Commands.ApproveReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ SECURITY: Audit için ApprovedByUserId eklendi
public record ApproveReviewCommand(
    Guid ReviewId,
    Guid ApprovedByUserId // Admin user ID for audit
) : IRequest<bool>;
