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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

public class GetWorstPerformersQueryHandler(
    IDbContext context,
    ILogger<GetWorstPerformersQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetWorstPerformersQuery, List<TopProductDto>>
{

    public async Task<List<TopProductDto>> Handle(GetWorstPerformersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching worst performers. Limit: {Limit}", request.Limit);

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
            .OrderBy(p => p.Revenue)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

