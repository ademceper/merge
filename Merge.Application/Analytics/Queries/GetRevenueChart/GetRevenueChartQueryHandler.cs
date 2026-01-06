using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetRevenueChart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetRevenueChartQueryHandler : IRequestHandler<GetRevenueChartQuery, RevenueChartDto>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetRevenueChartQueryHandler> _logger;

    public GetRevenueChartQueryHandler(
        IAdminService adminService,
        ILogger<GetRevenueChartQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<RevenueChartDto> Handle(GetRevenueChartQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching revenue chart. Days: {Days}", request.Days);

        return await _adminService.GetRevenueChartAsync(request.Days, cancellationToken);
    }
}

