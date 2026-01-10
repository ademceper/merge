using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.ML.Queries.GetPriceRecommendation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPriceRecommendationQueryHandler : IRequestHandler<GetPriceRecommendationQuery, PriceRecommendationDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetPriceRecommendationQueryHandler> _logger;
    private readonly PriceOptimizationHelper _helper;

    public GetPriceRecommendationQueryHandler(
        IDbContext context,
        ILogger<GetPriceRecommendationQueryHandler> logger,
        PriceOptimizationHelper helper)
    {
        _context = context;
        _logger = logger;
        _helper = helper;
    }

    public async Task<PriceRecommendationDto> Handle(GetPriceRecommendationQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting price recommendation. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
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

        _logger.LogInformation("Price recommendation retrieved. ProductId: {ProductId}, OptimalPrice: {OptimalPrice}, Confidence: {Confidence}",
            request.ProductId, recommendation.OptimalPrice, recommendation.Confidence);

        return recommendation;
    }
}
