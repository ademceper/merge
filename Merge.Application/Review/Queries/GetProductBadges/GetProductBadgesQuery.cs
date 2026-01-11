using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Queries.GetProductBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductBadgesQuery(
    Guid ProductId
) : IRequest<IEnumerable<ProductTrustBadgeDto>>;
