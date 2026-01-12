using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetSellerBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerBadgesQuery(
    Guid SellerId
) : IRequest<IEnumerable<SellerTrustBadgeDto>>;
