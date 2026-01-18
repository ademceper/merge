using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetReviewAnalytics;

public record GetReviewAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ReviewAnalyticsDto>;

