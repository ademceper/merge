using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.ForecastDemandForCategory;

public class ForecastDemandForCategoryQueryHandler(IDbContext context, ILogger<ForecastDemandForCategoryQueryHandler> logger, IOptions<MLSettings> mlSettings, IOptions<PaginationSettings> paginationSettings, DemandForecastingHelper helper) : IRequestHandler<ForecastDemandForCategoryQuery, PagedResult<DemandForecastDto>>
{
    private readonly MLSettings mlConfig = mlSettings.Value;
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<DemandForecastDto>> Handle(ForecastDemandForCategoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Forecasting demand for category. CategoryId: {CategoryId}, ForecastDays: {ForecastDays}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.ForecastDays, request.Page, request.PageSize);

        var forecastDays = request.ForecastDays;
        if (forecastDays > mlConfig.DemandForecastMaxDays) forecastDays = mlConfig.DemandForecastMaxDays;
        if (forecastDays < mlConfig.DemandForecastMinDays) forecastDays = mlConfig.DemandForecastMinDays;

        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        var productIds = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var allHistoricalSales = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => productIds.Contains(oi.ProductId))
            .GroupBy(oi => new { oi.ProductId, Date = oi.Order.CreatedAt.Date })
            .Select(g => new
            {
                ProductId = g.Key.ProductId,
                Date = g.Key.Date,
                Quantity = g.Sum(oi => oi.Quantity)
            })
            .ToListAsync(cancellationToken);

        // Not: Bu durumda anonymous type kullanılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        var salesByProduct = allHistoricalSales
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Date).ToList());

        List<DemandForecastDto> results = [];

        foreach (var product in products)
        {
            var historicalSales = salesByProduct.TryGetValue(product.Id, out var sales) 
                ? sales.Cast<object>().ToList() 
                : new List<object>();

            var forecast = helper.CalculateDemandForecast(product, historicalSales, forecastDays);

            results.Add(new DemandForecastDto(
                product.Id,
                product.Name,
                forecastDays,
                forecast.ForecastedQuantity,
                forecast.MinQuantity,
                forecast.MaxQuantity,
                forecast.Confidence,
                forecast.DailyForecast,
                forecast.Reasoning,
                DateTime.UtcNow
            ));
        }

        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        var orderedResults = results.OrderByDescending(r => r.ForecastedQuantity).ToList();

        var totalCount = orderedResults.Count;
        var pagedForecasts = orderedResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        logger.LogInformation("Demand forecast for category completed. CategoryId: {CategoryId}, TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, totalCount, page, pageSize);

        return new PagedResult<DemandForecastDto>
        {
            Items = pagedForecasts,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
