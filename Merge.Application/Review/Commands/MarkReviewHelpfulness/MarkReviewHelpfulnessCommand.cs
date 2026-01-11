using MediatR;

namespace Merge.Application.Review.Commands.MarkReviewHelpfulness;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkReviewHelpfulnessCommand(
    Guid UserId,
    Guid ReviewId,
    bool IsHelpful
) : IRequest;
