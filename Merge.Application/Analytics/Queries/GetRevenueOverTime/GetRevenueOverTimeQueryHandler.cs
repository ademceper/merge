using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetRevenueOverTime;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetRevenueOverTimeQueryHandler : IRequestHandler<GetRevenueOverTimeQuery, List<TimeSeriesDataPoint>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetRevenueOverTimeQueryHandler> _logger;

    public GetRevenueOverTimeQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetRevenueOverTimeQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<TimeSeriesDataPoint>> Handle(GetRevenueOverTimeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching revenue over time. StartDate: {StartDate}, EndDate: {EndDate}, Interval: {Interval}",
            request.StartDate, request.EndDate, request.Interval);

        return await _analyticsService.GetRevenueOverTimeAsync(request.StartDate, request.EndDate, request.Interval, cancellationToken);
    }
}

