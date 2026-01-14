using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetRatingDistribution;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRatingDistributionQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<RatingDistributionDto>>;

