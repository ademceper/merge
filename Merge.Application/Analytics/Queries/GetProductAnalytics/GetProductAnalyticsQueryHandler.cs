using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetProductAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetProductAnalyticsQueryHandler : IRequestHandler<GetProductAnalyticsQuery, ProductAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetProductAnalyticsQueryHandler> _logger;

    public GetProductAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetProductAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<ProductAnalyticsDto> Handle(GetProductAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetProductAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

