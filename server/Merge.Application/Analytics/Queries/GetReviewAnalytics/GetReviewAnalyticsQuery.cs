using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetReviewAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReviewAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ReviewAnalyticsDto>;

