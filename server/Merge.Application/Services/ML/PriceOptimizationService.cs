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

public class PriceOptimizationService : IPriceOptimizationService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PriceOptimizationService> _logger;
    private readonly MLSettings _mlSettings;
    private readonly PriceOptimizationHelper _helper;

    public PriceOptimizationService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<PriceOptimizationService> logger,
        IOptions<MLSettings> mlSettings,
        PriceOptimizationHelper helper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mlSettings = mlSettings.Value;
        _helper = helper;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PriceOptimizationDto> OptimizePriceAsync(Guid productId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        var similarProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: Helper kullan (business logic helper'da)
        var recommendation = await _helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);

        // Fiyatı güncelle (opsiyonel - sadece öneri döndürmek için kullanılabilir)
        if (request?.ApplyOptimization == true)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var oldPrice = product.Price;
            var newPrice = new Money(recommendation.OptimalPrice);
            product.SetPrice(newPrice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            _logger.LogInformation(
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PriceOptimizationDto>> OptimizePricesForCategoryAsync(Guid categoryId, PriceOptimizationRequestDto? request = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load competitor prices (N+1 fix)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() ve Distinct() YASAK - Database'de Select ve Distinct yap
        var categoryIds = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Select(p => p.CategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        var allSimilarProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => categoryIds.Contains(p.CategoryId) && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası GroupBy() ve ToDictionary() YASAK
        // Not: Bu durumda entity grouping yapılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        var similarProductsByCategory = allSimilarProducts
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<PriceOptimizationDto>();

        foreach (var product in products)
        {
            // ✅ PERFORMANCE: Memory'den similar products al (N+1 fix)
            // ✅ PERFORMANCE: ToListAsync() sonrası Where() ve ToList() YASAK
            // Not: Bu durumda `similar` zaten memory'de (dictionary'den geliyor), bu yüzden bu minimal bir işlem
            // Ancak business logic için gerekli (aynı product'ı exclude etmek için)
            var similarProducts = similarProductsByCategory.TryGetValue(product.CategoryId, out var similar) 
                ? similar.Where(p => p.Id != product.Id).ToList() 
                : new List<ProductEntity>();
            
            var recommendation = await _helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
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

        // ✅ PERFORMANCE: ToListAsync() sonrası OrderByDescending() YASAK
        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        return results.OrderByDescending(r => r.ExpectedRevenueChange);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PriceRecommendationDto> GetPriceRecommendationAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        var similarProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        return await _helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PriceRecommendationDto>> GetPriceRecommendationsAsync(Guid productId, int count = 5, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        var similarProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        var recommendation = await _helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
        return new[] { recommendation };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PriceOptimizationStatsDto> GetOptimizationStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        // Bu basit implementasyonda, gerçek optimizasyon istatistikleri tutulmuyor
        // Gerçek implementasyonda bir PriceOptimizationHistory tablosu olmalı

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive, cancellationToken);

        var productsWithDiscount = await _context.Set<ProductEntity>()
            .CountAsync(p => p.IsActive && p.DiscountPrice.HasValue, cancellationToken);

        return new PriceOptimizationStatsDto(
            productsWithDiscount,
            0, // AverageRevenueIncrease - Gerçek implementasyonda hesaplanmalı
            productsWithDiscount,
            start,
            end
        );
    }

    // ✅ ARCHITECTURE: Business logic helper'a taşındı (PriceOptimizationHelper)
}


