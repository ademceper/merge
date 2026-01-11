using MediatR;

namespace Merge.Application.Review.Commands.EvaluateAndAwardBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluateAndAwardBadgesCommand(
    Guid? SellerId = null
) : IRequest;
