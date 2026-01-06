using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<LowStockProductDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetLowStockProductsQueryHandler> _logger;

    public GetLowStockProductsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetLowStockProductsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<LowStockProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching low stock products. Threshold: {Threshold}", request.Threshold);

        return await _analyticsService.GetLowStockProductsAsync(request.Threshold, cancellationToken);
    }
}

