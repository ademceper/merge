using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAdminLowStockProductsQueryHandler : IRequestHandler<GetAdminLowStockProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetAdminLowStockProductsQueryHandler> _logger;

    public GetAdminLowStockProductsQueryHandler(
        IAdminService adminService,
        ILogger<GetAdminLowStockProductsQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAdminLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching admin low stock products. Threshold: {Threshold}", request.Threshold);

        return await _adminService.GetLowStockProductsAsync(request.Threshold, cancellationToken);
    }
}

