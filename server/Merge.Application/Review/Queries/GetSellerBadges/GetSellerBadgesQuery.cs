using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetSellerBadges;

public record GetSellerBadgesQuery(
    Guid SellerId
) : IRequest<IEnumerable<SellerTrustBadgeDto>>;
