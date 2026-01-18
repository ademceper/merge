using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetReviewHelpfulnessStats;

public record GetReviewHelpfulnessStatsQuery(
    Guid ReviewId,
    Guid? UserId = null
) : IRequest<ReviewHelpfulnessStatsDto>;
