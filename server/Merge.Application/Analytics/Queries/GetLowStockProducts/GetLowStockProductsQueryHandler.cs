using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetLowStockProducts;

public class GetLowStockProductsQueryHandler(
    IDbContext context,
    ILogger<GetLowStockProductsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetLowStockProductsQuery, List<LowStockProductDto>>
{

    public async Task<List<LowStockProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching low stock products. Threshold: {Threshold}", request.Threshold);

        var threshold = request.Threshold <= 0 ? settings.Value.DefaultLowStockThreshold : request.Threshold;
        
        return await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.StockQuantity < threshold && p.StockQuantity > 0)
            .Select(p => new LowStockProductDto(
                p.Id,
                p.Name,
                p.SKU,
                p.StockQuantity,
                threshold,
                threshold * 2
            ))
            .OrderBy(p => p.CurrentStock)
            .Take(settings.Value.MaxQueryLimit)
            .ToListAsync(cancellationToken);
    }
}

