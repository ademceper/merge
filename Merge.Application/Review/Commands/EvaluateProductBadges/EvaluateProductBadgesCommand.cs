using MediatR;

namespace Merge.Application.Review.Commands.EvaluateProductBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluateProductBadgesCommand(
    Guid ProductId
) : IRequest;
