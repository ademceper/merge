using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.ML;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Analytics;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.ML;

public class PriceOptimizationService(IDbContext context, IUnitOfWork unitOfWork, ILogger<PriceOptimizationService> logger, IOptions<MLSettings> mlSettings, PriceOptimizationHelper helper) : IPriceOptimizationService
{

    public async Task<PriceOptimizationDto> OptimizePriceAsync(Guid productId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        var similarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        var recommendation = await helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);

        // Fiyatı güncelle (opsiyonel - sadece öneri döndürmek için kullanılabilir)
        if (request?.ApplyOptimization == true)
        {
            var oldPrice = product.Price;
            var newPrice = new Money(recommendation.OptimalPrice);
            product.SetPrice(newPrice);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Price optimized for product. ProductId: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                productId, oldPrice, recommendation.OptimalPrice);
        }

        return new PriceOptimizationDto(
            productId,
            product.Name,
            product.Price,
            recommendation.OptimalPrice,
            recommendation.MinPrice,
            recommendation.MaxPrice,
            recommendation.ExpectedRevenueChange,
            recommendation.ExpectedSalesChange,
            recommendation.Confidence,
            recommendation.Reasoning,
            DateTime.UtcNow
        );
    }

    public async Task<IEnumerable<PriceOptimizationDto>> OptimizePricesForCategoryAsync(Guid categoryId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        var categoryIds = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Select(p => p.CategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        var allSimilarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => categoryIds.Contains(p.CategoryId) && p.IsActive)
            .ToListAsync(cancellationToken);

        // Not: Bu durumda entity grouping yapılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        var similarProductsByCategory = allSimilarProducts
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<PriceOptimizationDto> results = [];

        foreach (var product in products)
        {
            // Not: Bu durumda `similar` zaten memory'de (dictionary'den geliyor), bu yüzden bu minimal bir işlem
            // Ancak business logic için gerekli (aynı product'ı exclude etmek için)
            var similarProducts = similarProductsByCategory.TryGetValue(product.CategoryId, out var similar) 
                ? similar.Where(p => p.Id != product.Id).ToList() 
                : [];
            
            var recommendation = await helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
            results.Add(new PriceOptimizationDto(
                product.Id,
                product.Name,
                product.Price,
                recommendation.OptimalPrice,
                recommendation.MinPrice,
                recommendation.MaxPrice,
                recommendation.ExpectedRevenueChange,
                recommendation.ExpectedSalesChange,
                recommendation.Confidence,
                recommendation.Reasoning,
                DateTime.UtcNow
            ));
        }

        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        return results.OrderByDescending(r => r.ExpectedRevenueChange);
    }

    public async Task<PriceRecommendationDto> GetPriceRecommendationAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        var similarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        return await helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
    }

    public async Task<IEnumerable<PriceRecommendationDto>> GetPriceRecommendationsAsync(Guid productId, int count = 5, CancellationToken cancellationToken = default)
    {
        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        var similarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        var recommendation = await helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
        return new[] { recommendation };
    }

    public async Task<PriceOptimizationStatsDto> GetOptimizationStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        // Bu basit implementasyonda, gerçek optimizasyon istatistikleri tutulmuyor
        // Gerçek implementasyonda bir PriceOptimizationHistory tablosu olmalı

        var totalProducts = await context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive, cancellationToken);

        var productsWithDiscount = await context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive && p.DiscountPrice.HasValue, cancellationToken);

        return new PriceOptimizationStatsDto(
            productsWithDiscount,
            0, // AverageRevenueIncrease - Gerçek implementasyonda hesaplanmalı
            productsWithDiscount,
            start,
            end
        );
    }

}

