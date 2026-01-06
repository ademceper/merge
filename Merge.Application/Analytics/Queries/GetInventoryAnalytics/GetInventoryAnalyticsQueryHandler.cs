using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetInventoryAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetInventoryAnalyticsQueryHandler : IRequestHandler<GetInventoryAnalyticsQuery, InventoryAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetInventoryAnalyticsQueryHandler> _logger;

    public GetInventoryAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetInventoryAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<InventoryAnalyticsDto> Handle(GetInventoryAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching inventory analytics");

        return await _analyticsService.GetInventoryAnalyticsAsync(cancellationToken);
    }
}

