using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetReports;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetReportsQueryHandler> _logger;

    public GetReportsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetReportsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching reports. UserId: {UserId}, Type: {Type}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Type, request.Page, request.PageSize);

        return await _analyticsService.GetReportsAsync(request.UserId, request.Type, request.Page, request.PageSize, cancellationToken);
    }
}

