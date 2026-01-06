using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetInventoryOverview;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetInventoryOverviewQueryHandler : IRequestHandler<GetInventoryOverviewQuery, InventoryOverviewDto>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetInventoryOverviewQueryHandler> _logger;

    public GetInventoryOverviewQueryHandler(
        IAdminService adminService,
        ILogger<GetInventoryOverviewQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<InventoryOverviewDto> Handle(GetInventoryOverviewQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching inventory overview");

        return await _adminService.GetInventoryOverviewAsync(cancellationToken);
    }
}

