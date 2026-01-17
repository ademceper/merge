using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;
using Merge.Application.ML.Helpers;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.ForecastDemand;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ForecastDemandCommandHandler(IDbContext context, ILogger<ForecastDemandCommandHandler> logger, IOptions<MLSettings> mlSettings, DemandForecastingHelper helper) : IRequestHandler<ForecastDemandCommand, DemandForecastDto>
{
    private readonly MLSettings mlConfig = mlSettings.Value;

    public async Task<DemandForecastDto> Handle(ForecastDemandCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Forecasting demand. ProductId: {ProductId}, ForecastDays: {ForecastDays}",
            request.ProductId, request.ForecastDays);

        // ✅ BOLUM 3.4: Unbounded query koruması - forecastDays limiti
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var forecastDays = request.ForecastDays;
        if (forecastDays > mlConfig.DemandForecastMaxDays) forecastDays = mlConfig.DemandForecastMaxDays;
        if (forecastDays < mlConfig.DemandForecastMinDays) forecastDays = mlConfig.DemandForecastMinDays;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var historicalSales = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.ProductId == request.ProductId)
            .GroupBy(oi => oi.Order.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Quantity = g.Sum(oi => oi.Quantity)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        // Basit talep tahmin algoritması
        var forecast = helper.CalculateDemandForecast(product, historicalSales.Cast<object>().ToList(), forecastDays);

        logger.LogInformation("Demand forecast completed. ProductId: {ProductId}, ForecastDays: {ForecastDays}",
            request.ProductId, forecastDays);

        return new DemandForecastDto(
            request.ProductId,
            product.Name,
            forecastDays,
            forecast.ForecastedQuantity,
            forecast.MinQuantity,
            forecast.MaxQuantity,
            forecast.Confidence,
            forecast.DailyForecast,
            forecast.Reasoning,
            DateTime.UtcNow
        );
    }
}
