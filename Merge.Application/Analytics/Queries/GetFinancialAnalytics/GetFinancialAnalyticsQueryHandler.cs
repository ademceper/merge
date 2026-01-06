using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetFinancialAnalyticsQueryHandler : IRequestHandler<GetFinancialAnalyticsQuery, FinancialAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetFinancialAnalyticsQueryHandler> _logger;

    public GetFinancialAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetFinancialAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<FinancialAnalyticsDto> Handle(GetFinancialAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching financial analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetFinancialAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

