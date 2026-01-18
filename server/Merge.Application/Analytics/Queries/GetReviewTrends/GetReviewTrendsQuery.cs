using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetReviewTrends;

public record GetReviewTrendsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<List<ReviewTrendDto>>;

