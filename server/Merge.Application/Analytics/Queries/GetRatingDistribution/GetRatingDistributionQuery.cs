using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetRatingDistribution;

public record GetRatingDistributionQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<RatingDistributionDto>>;

