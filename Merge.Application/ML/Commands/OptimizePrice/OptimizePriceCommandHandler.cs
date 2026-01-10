using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.ML.Commands.OptimizePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class OptimizePriceCommandHandler : IRequestHandler<OptimizePriceCommand, PriceOptimizationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OptimizePriceCommandHandler> _logger;
    private readonly MLSettings _mlSettings;
    private readonly PriceOptimizationHelper _helper;

    public OptimizePriceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<OptimizePriceCommandHandler> logger,
        IOptions<MLSettings> mlSettings,
        PriceOptimizationHelper helper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mlSettings = mlSettings.Value;
        _helper = helper;
    }

    public async Task<PriceOptimizationDto> Handle(OptimizePriceCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Optimizing price. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: Tracking gerekli (product güncellenebilir) + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            throw new NotFoundException("Ürün", request.ProductId);
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

        // ✅ BOLUM 12.0: Configuration - Request'teki MinPrice/MaxPrice kontrolü
        if (request.Request?.MinPrice.HasValue == true && recommendation.OptimalPrice < request.Request.MinPrice.Value)
        {
            _logger.LogWarning("Optimal price {OptimalPrice} is below requested minimum {MinPrice}. Using minimum price.",
                recommendation.OptimalPrice, request.Request.MinPrice.Value);
            recommendation = new PriceRecommendationDto(
                request.Request.MinPrice.Value,
                recommendation.MinPrice,
                recommendation.MaxPrice,
                recommendation.Confidence,
                recommendation.ExpectedRevenueChange,
                recommendation.ExpectedSalesChange,
                recommendation.Reasoning + $" Adjusted to minimum price {request.Request.MinPrice.Value:C}.");
        }

        if (request.Request?.MaxPrice.HasValue == true && recommendation.OptimalPrice > request.Request.MaxPrice.Value)
        {
            _logger.LogWarning("Optimal price {OptimalPrice} is above requested maximum {MaxPrice}. Using maximum price.",
                recommendation.OptimalPrice, request.Request.MaxPrice.Value);
            recommendation = new PriceRecommendationDto(
                request.Request.MaxPrice.Value,
                recommendation.MinPrice,
                recommendation.MaxPrice,
                recommendation.Confidence,
                recommendation.ExpectedRevenueChange,
                recommendation.ExpectedSalesChange,
                recommendation.Reasoning + $" Adjusted to maximum price {request.Request.MaxPrice.Value:C}.");
        }

        // Fiyatı güncelle (opsiyonel - sadece öneri döndürmek için kullanılabilir)
        if (request.Request?.ApplyOptimization == true)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var oldPrice = product.Price;
            var newPrice = new Money(recommendation.OptimalPrice);
            product.SetPrice(newPrice);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            _logger.LogInformation(
                "Price optimized for product. ProductId: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                request.ProductId, oldPrice, recommendation.OptimalPrice);
        }

        var result = new PriceOptimizationDto(
            request.ProductId,
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

        _logger.LogInformation("Price optimized. ProductId: {ProductId}, RecommendedPrice: {RecommendedPrice}, Confidence: {Confidence}",
            request.ProductId, result.RecommendedPrice, result.Confidence);

        return result;
    }
}
