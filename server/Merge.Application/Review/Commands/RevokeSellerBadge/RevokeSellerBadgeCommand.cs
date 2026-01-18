using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RevokeSellerBadge;

public record RevokeSellerBadgeCommand(
    Guid SellerId,
    Guid BadgeId
) : IRequest<bool>;
