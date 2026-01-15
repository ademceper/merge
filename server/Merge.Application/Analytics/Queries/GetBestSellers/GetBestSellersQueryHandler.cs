using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Analytics.Queries.GetBestSellers;

public class GetBestSellersQueryHandler(
    IDbContext context,
    ILogger<GetBestSellersQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetBestSellersQuery, List<TopProductDto>>
{

    public async Task<List<TopProductDto>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching best sellers. Limit: {Limit}", request.Limit);

        var limit = request.Limit == 10 ? settings.Value.TopProductsLimit : request.Limit;
        
        var last30Days = DateTime.UtcNow.AddDays(-settings.Value.DefaultPeriodDays);
        
        return await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= last30Days)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.Name,
                g.Key.SKU,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.TotalPrice),
                g.Average(oi => oi.UnitPrice)
            ))
            .OrderByDescending(p => p.Revenue)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

