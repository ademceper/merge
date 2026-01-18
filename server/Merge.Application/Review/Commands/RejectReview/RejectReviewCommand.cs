using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RejectReview;

public record RejectReviewCommand(
    Guid ReviewId,
    Guid RejectedByUserId, // Admin user ID for audit
    string? Reason
) : IRequest<bool>;
