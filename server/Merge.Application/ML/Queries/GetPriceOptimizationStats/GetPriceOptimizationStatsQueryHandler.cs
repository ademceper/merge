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

public class GetPriceOptimizationStatsQueryHandler(IDbContext context, ILogger<GetPriceOptimizationStatsQueryHandler> logger, IOptions<MLSettings> mlSettings) : IRequestHandler<GetPriceOptimizationStatsQuery, PriceOptimizationStatsDto>
{
    private readonly MLSettings mlConfig = mlSettings.Value;

    public async Task<PriceOptimizationStatsDto> Handle(GetPriceOptimizationStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting price optimization stats. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-mlConfig.DefaultAnalysisPeriodDays);
        var end = request.EndDate ?? DateTime.UtcNow;

        // Bu basit implementasyonda, gerçek optimizasyon istatistikleri tutulmuyor
        // Gerçek implementasyonda bir PriceOptimizationHistory tablosu olmalı

        var totalProducts = await context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive, cancellationToken);

        var productsWithDiscount = await context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive && p.DiscountPrice.HasValue, cancellationToken);

        logger.LogInformation("Price optimization stats retrieved successfully.");

        return new PriceOptimizationStatsDto(
            productsWithDiscount,
            0, // AverageRevenueIncrease - Gerçek implementasyonda hesaplanmalı
            productsWithDiscount,
            start,
            end
        );
    }
}
