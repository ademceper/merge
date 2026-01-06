using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetSalesAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSalesAnalyticsQueryHandler : IRequestHandler<GetSalesAnalyticsQuery, SalesAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetSalesAnalyticsQueryHandler> _logger;

    public GetSalesAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetSalesAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<SalesAnalyticsDto> Handle(GetSalesAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching sales analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetSalesAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

