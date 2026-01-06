using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetFinancialMetricsQueryHandler : IRequestHandler<GetFinancialMetricsQuery, FinancialMetricsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetFinancialMetricsQueryHandler> _logger;

    public GetFinancialMetricsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetFinancialMetricsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<FinancialMetricsDto> Handle(GetFinancialMetricsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching financial metrics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetFinancialMetricsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

