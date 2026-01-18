using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetAdminTopProducts;

public class GetAdminTopProductsQueryHandler(
    IDbContext context,
    ILogger<GetAdminTopProductsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetAdminTopProductsQuery, IEnumerable<AdminTopProductDto>>
{

    public async Task<IEnumerable<AdminTopProductDto>> Handle(GetAdminTopProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top products. Count: {Count}", request.Count);
        
        var count = request.Count == 10 ? settings.Value.TopProductsLimit : request.Count;
        
        var topProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.ImageUrl })
            .Select(g => new AdminTopProductDto(
                g.Key.ProductId,
                g.Key.Name ?? string.Empty,
                g.Key.ImageUrl ?? string.Empty,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.TotalPrice)
            ))
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Top products fetched. Count: {Count}, ProductsReturned: {ProductsReturned}", count, topProducts.Count);

        return topProducts;
    }
}

