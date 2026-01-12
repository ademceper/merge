using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RejectReview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ SECURITY: Audit için RejectedByUserId eklendi
public record RejectReviewCommand(
    Guid ReviewId,
    Guid RejectedByUserId, // Admin user ID for audit
    string? Reason
) : IRequest<bool>;
