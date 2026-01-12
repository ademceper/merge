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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ForecastDemandForCategoryQueryHandler : IRequestHandler<ForecastDemandForCategoryQuery, PagedResult<DemandForecastDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<ForecastDemandForCategoryQueryHandler> _logger;
    private readonly MLSettings _mlSettings;
    private readonly PaginationSettings _paginationSettings;
    private readonly DemandForecastingHelper _helper;

    public ForecastDemandForCategoryQueryHandler(
        IDbContext context,
        ILogger<ForecastDemandForCategoryQueryHandler> logger,
        IOptions<MLSettings> mlSettings,
        IOptions<PaginationSettings> paginationSettings,
        DemandForecastingHelper helper)
    {
        _context = context;
        _logger = logger;
        _mlSettings = mlSettings.Value;
        _paginationSettings = paginationSettings.Value;
        _helper = helper;
    }

    public async Task<PagedResult<DemandForecastDto>> Handle(ForecastDemandForCategoryQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Forecasting demand for category. CategoryId: {CategoryId}, ForecastDays: {ForecastDays}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.ForecastDays, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Unbounded query koruması - forecastDays limiti
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var forecastDays = request.ForecastDays;
        if (forecastDays > _mlSettings.DemandForecastMaxDays) forecastDays = _mlSettings.DemandForecastMaxDays;
        if (forecastDays < _mlSettings.DemandForecastMinDays) forecastDays = _mlSettings.DemandForecastMinDays;

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load historical sales (N+1 fix)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - Database'de Select yap
        var productIds = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var allHistoricalSales = await _context.Set<OrderItem>()
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

        // ✅ PERFORMANCE: ToListAsync() sonrası GroupBy() ve ToDictionary() YASAK
        // Not: Bu durumda anonymous type kullanılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        var salesByProduct = allHistoricalSales
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Date).ToList());

        var results = new List<DemandForecastDto>();

        foreach (var product in products)
        {
            var historicalSales = salesByProduct.TryGetValue(product.Id, out var sales) 
                ? sales.Cast<object>().ToList() 
                : new List<object>();

            var forecast = _helper.CalculateDemandForecast(product, historicalSales, forecastDays);

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

        // ✅ PERFORMANCE: ToListAsync() sonrası OrderByDescending() YASAK
        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        var orderedResults = results.OrderByDescending(r => r.ForecastedQuantity).ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = orderedResults.Count;
        var pagedForecasts = orderedResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("Demand forecast for category completed. CategoryId: {CategoryId}, TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
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
