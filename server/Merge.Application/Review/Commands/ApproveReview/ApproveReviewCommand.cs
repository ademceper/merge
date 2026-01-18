using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.ApproveReview;

public record ApproveReviewCommand(
    Guid ReviewId,
    Guid ApprovedByUserId // Admin user ID for audit
) : IRequest<bool>;
