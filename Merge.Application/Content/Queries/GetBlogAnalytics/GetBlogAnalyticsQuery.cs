using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Content.Queries.GetBlogAnalytics;

public record GetBlogAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<BlogAnalyticsDto>;

