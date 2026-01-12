using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RemoveReviewHelpfulness;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveReviewHelpfulnessCommand(
    Guid UserId,
    Guid ReviewId
) : IRequest;
