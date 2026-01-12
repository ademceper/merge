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

namespace Merge.Application.Analytics.Queries.GetTopProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTopProductsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetTopProductsQueryHandler(
        IDbContext context,
        ILogger<GetTopProductsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top products. StartDate: {StartDate}, EndDate: {EndDate}, Limit: {Limit}",
            request.StartDate, request.EndDate, request.Limit);

        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var limit = request.Limit == 10 ? _settings.TopProductsLimit : request.Limit;
        
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted and !oi.Order.IsDeleted checks (Global Query Filter handles it)
        return await _context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= request.StartDate && oi.Order.CreatedAt <= request.EndDate)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
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

