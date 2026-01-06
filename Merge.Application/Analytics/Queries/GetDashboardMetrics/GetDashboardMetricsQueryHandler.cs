using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetDashboardMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, List<DashboardMetricDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetDashboardMetricsQueryHandler> _logger;

    public GetDashboardMetricsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetDashboardMetricsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<DashboardMetricDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard metrics. Category: {Category}", request.Category);

        return await _analyticsService.GetDashboardMetricsAsync(request.Category, cancellationToken);
    }
}

