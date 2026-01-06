using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetWorstPerformersQueryHandler : IRequestHandler<GetWorstPerformersQuery, List<TopProductDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetWorstPerformersQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetWorstPerformersQueryHandler(
        IDbContext context,
        ILogger<GetWorstPerformersQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<List<TopProductDto>> Handle(GetWorstPerformersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching worst performers. Limit: {Limit}", request.Limit);

        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var limit = request.Limit == 10 ? _settings.TopProductsLimit : request.Limit;
        
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var last30Days = DateTime.UtcNow.AddDays(-_settings.DefaultPeriodDays);
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted and !oi.Order.IsDeleted checks (Global Query Filter handles it)
        return await _context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= last30Days)
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
            .OrderBy(p => p.Revenue)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

