using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Content.Queries.GetLandingPageAnalytics;

public record GetLandingPageAnalyticsQuery(
    Guid Id,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<LandingPageAnalyticsDto>;

