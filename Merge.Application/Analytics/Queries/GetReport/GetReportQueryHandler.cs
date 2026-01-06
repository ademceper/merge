using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportDto?>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetReportQueryHandler> _logger;

    public GetReportQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetReportQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<ReportDto?> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);

        var report = await _analyticsService.GetReportAsync(request.Id, cancellationToken);

        // ✅ SECURITY: Authorization check - Users can only view their own reports unless Admin
        // Bu kontrol controller'da yapılıyor, burada sadece data getiriyoruz
        return report;
    }
}

