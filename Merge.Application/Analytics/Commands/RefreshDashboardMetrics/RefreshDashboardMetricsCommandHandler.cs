using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.RefreshDashboardMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RefreshDashboardMetricsCommandHandler : IRequestHandler<RefreshDashboardMetricsCommand>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<RefreshDashboardMetricsCommandHandler> _logger;

    public RefreshDashboardMetricsCommandHandler(
        IAnalyticsService analyticsService,
        ILogger<RefreshDashboardMetricsCommandHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task Handle(RefreshDashboardMetricsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refreshing dashboard metrics");

        await _analyticsService.RefreshDashboardMetricsAsync(cancellationToken);
    }
}

