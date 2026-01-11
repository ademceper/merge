using MediatR;

namespace Merge.Application.Review.Commands.RevokeProductBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RevokeProductBadgeCommand(
    Guid ProductId,
    Guid BadgeId
) : IRequest<bool>;
