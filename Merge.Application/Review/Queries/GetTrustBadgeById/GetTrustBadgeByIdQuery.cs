using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Queries.GetTrustBadgeById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTrustBadgeByIdQuery(
    Guid BadgeId
) : IRequest<TrustBadgeDto?>;
