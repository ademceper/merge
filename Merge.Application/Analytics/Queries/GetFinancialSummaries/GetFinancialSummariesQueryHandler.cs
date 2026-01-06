using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialSummaries;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetFinancialSummariesQueryHandler : IRequestHandler<GetFinancialSummariesQuery, List<FinancialSummaryDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetFinancialSummariesQueryHandler> _logger;

    public GetFinancialSummariesQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetFinancialSummariesQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<FinancialSummaryDto>> Handle(GetFinancialSummariesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching financial summaries. StartDate: {StartDate}, EndDate: {EndDate}, Period: {Period}",
            request.StartDate, request.EndDate, request.Period);

        return await _analyticsService.GetFinancialSummariesAsync(request.StartDate, request.EndDate, request.Period, cancellationToken);
    }
}

