using MediatR;

namespace Merge.Application.Review.Commands.RevokeSellerBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RevokeSellerBadgeCommand(
    Guid SellerId,
    Guid BadgeId
) : IRequest<bool>;
