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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAdminTopProductsQueryHandler(
    IDbContext context,
    ILogger<GetAdminTopProductsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetAdminTopProductsQuery, IEnumerable<AdminTopProductDto>>
{

    public async Task<IEnumerable<AdminTopProductDto>> Handle(GetAdminTopProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top products. Count: {Count}", request.Count);
        
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var count = request.Count == 10 ? settings.Value.TopProductsLimit : request.Count;
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted check (Global Query Filter handles it)
        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
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

