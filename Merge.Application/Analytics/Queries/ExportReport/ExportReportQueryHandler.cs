using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.ExportReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ExportReportQueryHandler : IRequestHandler<ExportReportQuery, byte[]?>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<ExportReportQueryHandler> _logger;

    public ExportReportQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<ExportReportQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<byte[]?> Handle(ExportReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);

        return await _analyticsService.ExportReportAsync(request.Id, request.UserId, cancellationToken);
    }
}

