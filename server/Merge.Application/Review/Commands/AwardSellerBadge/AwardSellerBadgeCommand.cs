using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.AwardSellerBadge;

public record AwardSellerBadgeCommand(
    Guid SellerId,
    Guid BadgeId,
    DateTime? ExpiresAt,
    string? AwardReason
) : IRequest<SellerTrustBadgeDto>;
