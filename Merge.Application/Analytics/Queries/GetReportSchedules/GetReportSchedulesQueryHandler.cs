using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetReportSchedules;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReportSchedulesQueryHandler : IRequestHandler<GetReportSchedulesQuery, PagedResult<ReportScheduleDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetReportSchedulesQueryHandler> _logger;

    public GetReportSchedulesQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetReportSchedulesQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<PagedResult<ReportScheduleDto>> Handle(GetReportSchedulesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching report schedules. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Page, request.PageSize);

        return await _analyticsService.GetReportSchedulesAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
    }
}

