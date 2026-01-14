using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.AwardProductBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AwardProductBadgeCommand(
    Guid ProductId,
    Guid BadgeId,
    DateTime? ExpiresAt,
    string? AwardReason
) : IRequest<ProductTrustBadgeDto>;
