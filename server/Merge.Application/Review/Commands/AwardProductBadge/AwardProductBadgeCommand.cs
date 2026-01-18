using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.AwardProductBadge;

public record AwardProductBadgeCommand(
    Guid ProductId,
    Guid BadgeId,
    DateTime? ExpiresAt,
    string? AwardReason
) : IRequest<ProductTrustBadgeDto>;
