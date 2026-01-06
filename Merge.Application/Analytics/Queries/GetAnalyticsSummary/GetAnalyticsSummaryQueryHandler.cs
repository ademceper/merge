using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetAnalyticsSummary;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetAnalyticsSummaryQueryHandler> _logger;

    public GetAnalyticsSummaryQueryHandler(
        IAdminService adminService,
        ILogger<GetAnalyticsSummaryQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<AnalyticsSummaryDto> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching analytics summary. Days: {Days}", request.Days);

        return await _adminService.GetAnalyticsSummaryAsync(request.Days, cancellationToken);
    }
}

