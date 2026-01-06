using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetMarketingAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetMarketingAnalyticsQueryHandler : IRequestHandler<GetMarketingAnalyticsQuery, MarketingAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetMarketingAnalyticsQueryHandler> _logger;

    public GetMarketingAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetMarketingAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<MarketingAnalyticsDto> Handle(GetMarketingAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching marketing analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetMarketingAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

