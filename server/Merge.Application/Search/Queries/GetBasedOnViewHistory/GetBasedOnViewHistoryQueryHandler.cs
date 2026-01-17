using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetBasedOnViewHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetBasedOnViewHistoryQueryHandler(IDbContext context, IMapper mapper, ILogger<GetBasedOnViewHistoryQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetBasedOnViewHistoryQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetBasedOnViewHistoryQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Based on view history recommendations isteniyor. UserId: {UserId}, MaxResults: {MaxResults}",
            request.UserId, request.MaxResults);

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var recentlyViewed = await context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(rv => rv.Product)
                .ThenInclude(p => p.Category)
            .Where(rv => rv.UserId == request.UserId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        if (recentlyViewed.Count == 0)
        {
            return Array.Empty<ProductRecommendationDto>();
        }

        // ✅ PERFORMANCE: recentlyViewed zaten materialize edilmiş küçük liste (5 item), bu yüzden ID'leri almak kabul edilebilir
        // Ancak category'ler için subquery kullanıyoruz (ISSUE #3.1 fix)
        var viewedProductIds = recentlyViewed.Select(rv => rv.ProductId).ToList();
        
        // Category'ler için subquery kullan (büyük olabilir)
        var recentlyViewedQuery = context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
            .Where(rv => rv.UserId == request.UserId)
            .OrderByDescending(rv => rv.ViewedAt)
            .Take(5);
        
        var viewedCategoriesSubquery = from rv in recentlyViewedQuery
                                      join p in context.Set<ProductEntity>().AsNoTracking() on rv.ProductId equals p.Id
                                      select p.CategoryId;

        // Get products from same categories, excluding already viewed
        var recommendations = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       viewedCategoriesSubquery.Distinct().Contains(p.CategoryId) &&
                       !viewedProductIds.Contains(p.Id))
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
                "Based on your browsing history",
                rec.Rating
            ))
            .ToList();

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Based on view history recommendations tamamlandı. UserId: {UserId}, Count: {Count}",
            request.UserId, recommendationDtos.Count);

        return recommendationDtos;
    }
}
