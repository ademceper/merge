using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetStockByWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetStockByWarehouseQueryHandler : IRequestHandler<GetStockByWarehouseQuery, List<WarehouseStockDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetStockByWarehouseQueryHandler> _logger;

    public GetStockByWarehouseQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetStockByWarehouseQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<WarehouseStockDto>> Handle(GetStockByWarehouseQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock by warehouse");

        return await _analyticsService.GetStockByWarehouseAsync(cancellationToken);
    }
}

