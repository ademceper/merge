using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetTrustBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTrustBadgesQuery(
    string? BadgeType = null
) : IRequest<IEnumerable<TrustBadgeDto>>;
