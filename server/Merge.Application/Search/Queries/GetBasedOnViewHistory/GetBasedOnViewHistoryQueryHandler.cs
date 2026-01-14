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
public class GetBasedOnViewHistoryQueryHandler : IRequestHandler<GetBasedOnViewHistoryQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBasedOnViewHistoryQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetBasedOnViewHistoryQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBasedOnViewHistoryQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetBasedOnViewHistoryQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Based on view history recommendations isteniyor. UserId: {UserId}, MaxResults: {MaxResults}",
            request.UserId, request.MaxResults);

        var maxResults = request.MaxResults > _searchSettings.MaxRecommendationResults
            ? _searchSettings.MaxRecommendationResults
            : request.MaxResults;

        // Get recently viewed products
        var recentlyViewed = await _context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
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

        var viewedCategories = recentlyViewed.Select(rv => rv.Product.CategoryId).Distinct().ToList();
        var viewedProductIds = recentlyViewed.Select(rv => rv.ProductId).ToList();

        // Get products from same categories, excluding already viewed
        var recommendations = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       viewedCategories.Contains(p.CategoryId) &&
                       !viewedProductIds.Contains(p.Id))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendationDtos = _mapper.Map<IEnumerable<ProductRecommendationDto>>(recommendations)
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
        _logger.LogInformation(
            "Based on view history recommendations tamamlandı. UserId: {UserId}, Count: {Count}",
            request.UserId, recommendationDtos.Count);

        return recommendationDtos;
    }
}
