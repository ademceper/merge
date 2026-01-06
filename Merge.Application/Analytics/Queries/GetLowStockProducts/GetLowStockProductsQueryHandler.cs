using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Analytics.Queries.GetLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<LowStockProductDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetLowStockProductsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetLowStockProductsQueryHandler(
        IDbContext context,
        ILogger<GetLowStockProductsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<List<LowStockProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching low stock products. Threshold: {Threshold}", request.Threshold);

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var threshold = request.Threshold <= 0 ? _settings.DefaultLowStockThreshold : request.Threshold;
        
        // ✅ PERFORMANCE: Database'de DTO oluştur (memory'de Select/ToList YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        return await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.StockQuantity < threshold && p.StockQuantity > 0)
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(p => new LowStockProductDto(
                p.Id,
                p.Name,
                p.SKU,
                p.StockQuantity,
                threshold,
                threshold * 2
            ))
            .OrderBy(p => p.CurrentStock)
            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
            .Take(_settings.MaxQueryLimit)
            .ToListAsync(cancellationToken);
    }
}

