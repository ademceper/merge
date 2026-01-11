using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Queries.GetReviewHelpfulnessStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReviewHelpfulnessStatsQuery(
    Guid ReviewId,
    Guid? UserId = null
) : IRequest<ReviewHelpfulnessStatsDto>;
