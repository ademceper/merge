using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetPriceOptimizationStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPriceOptimizationStatsQueryHandler : IRequestHandler<GetPriceOptimizationStatsQuery, PriceOptimizationStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetPriceOptimizationStatsQueryHandler> _logger;
    private readonly MLSettings _mlSettings;

    public GetPriceOptimizationStatsQueryHandler(
        IDbContext context,
        ILogger<GetPriceOptimizationStatsQueryHandler> logger,
        IOptions<MLSettings> mlSettings)
    {
        _context = context;
        _logger = logger;
        _mlSettings = mlSettings.Value;
    }

    public async Task<PriceOptimizationStatsDto> Handle(GetPriceOptimizationStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting price optimization stats. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-_mlSettings.DefaultAnalysisPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        // Bu basit implementasyonda, gerçek optimizasyon istatistikleri tutulmuyor
        // Gerçek implementasyonda bir PriceOptimizationHistory tablosu olmalı

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive, cancellationToken);

        var productsWithDiscount = await _context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive && p.DiscountPrice.HasValue, cancellationToken);

        _logger.LogInformation("Price optimization stats retrieved successfully.");

        return new PriceOptimizationStatsDto(
            productsWithDiscount,
            0, // AverageRevenueIncrease - Gerçek implementasyonda hesaplanmalı
            productsWithDiscount,
            start,
            end
        );
    }
}
