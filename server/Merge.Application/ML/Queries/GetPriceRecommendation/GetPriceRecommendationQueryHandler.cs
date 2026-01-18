using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetPriceRecommendation;

public class GetPriceRecommendationQueryHandler(IDbContext context, ILogger<GetPriceRecommendationQueryHandler> logger, PriceOptimizationHelper helper) : IRequestHandler<GetPriceRecommendationQuery, PriceRecommendationDto>
{

    public async Task<PriceRecommendationDto> Handle(GetPriceRecommendationQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting price recommendation. ProductId: {ProductId}", request.ProductId);

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found. ProductId: {ProductId}", request.ProductId);
            throw new NotFoundException("Ürün", request.ProductId);
        }

        var similarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && 
                       p.Id != product.Id && 
                       p.IsActive)
            .ToListAsync(cancellationToken);

        var recommendation = await helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);

        logger.LogInformation("Price recommendation retrieved. ProductId: {ProductId}, OptimalPrice: {OptimalPrice}, Confidence: {Confidence}",
            request.ProductId, recommendation.OptimalPrice, recommendation.Confidence);

        return recommendation;
    }
}
