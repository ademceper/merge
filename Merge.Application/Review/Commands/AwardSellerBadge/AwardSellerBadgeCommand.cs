using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Commands.AwardSellerBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AwardSellerBadgeCommand(
    Guid SellerId,
    Guid BadgeId,
    DateTime? ExpiresAt,
    string? AwardReason
) : IRequest<SellerTrustBadgeDto>;
