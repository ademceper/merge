using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.Search.Queries.GetTrendingProducts;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetPersonalizedRecommendations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPersonalizedRecommendationsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPersonalizedRecommendationsQueryHandler> logger, IMediator mediator, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetPersonalizedRecommendationsQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetPersonalizedRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Personalized recommendations isteniyor. UserId: {UserId}, MaxResults: {MaxResults}",
            request.UserId, request.MaxResults);

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        var userOrders = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.UserId == request.UserId
            select p.CategoryId
        ).Distinct().ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Single Include, AsSplitQuery not needed but keeping for consistency
        var wishlistCategories = await context.Set<Wishlist>()
            .AsNoTracking()
            .Include(w => w.Product)
            .Where(w => w.UserId == request.UserId)
            .Select(w => w.Product.CategoryId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var preferredCategories = userOrders.Union(wishlistCategories).ToList();

        if (preferredCategories.Count == 0)
        {
            // If no history, return trending products
            var trendingQuery = new GetTrendingProducts.GetTrendingProductsQuery(
                Days: searchConfig.DefaultTrendingDays,
                MaxResults: maxResults);
            return await mediator.Send(trendingQuery, cancellationToken);
        }

        // Get highly rated products from preferred categories
        var recommendations = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       preferredCategories.Contains(p.CategoryId) &&
                       p.Rating >= searchConfig.MinRatingForPersonalizedRecommendations)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendationDtos = mapper.Map<IEnumerable<ProductRecommendationDto>>(recommendations)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "Based on your interests",
                rec.Rating * rec.ReviewCount
            ))
            .ToList();

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Personalized recommendations tamamlandı. UserId: {UserId}, Count: {Count}",
            request.UserId, recommendationDtos.Count);

        return recommendationDtos;
    }
}
