using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetFinancialReportQueryHandler : IRequestHandler<GetFinancialReportQuery, FinancialReportDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetFinancialReportQueryHandler> _logger;

    public GetFinancialReportQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetFinancialReportQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<FinancialReportDto> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching financial report. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetFinancialReportAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

