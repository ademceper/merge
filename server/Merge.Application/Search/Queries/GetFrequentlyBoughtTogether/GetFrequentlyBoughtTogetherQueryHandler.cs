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
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetFrequentlyBoughtTogether;

public class GetFrequentlyBoughtTogetherQueryHandler(IDbContext context, IMapper mapper, ILogger<GetFrequentlyBoughtTogetherQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetFrequentlyBoughtTogetherQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetFrequentlyBoughtTogetherQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Frequently bought together isteniyor. ProductId: {ProductId}, MaxResults: {MaxResults}",
            request.ProductId, request.MaxResults);

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        var frequentlyBought = await (
            from oi1 in context.Set<OrderItem>().AsNoTracking()
            join oi2 in context.Set<OrderItem>().AsNoTracking()
                on oi1.OrderId equals oi2.OrderId
            where oi1.ProductId == request.ProductId && oi2.ProductId != request.ProductId
            group oi2 by oi2.ProductId into g
            orderby g.Count() descending
            select new
            {
                ProductId = g.Key,
                Count = g.Count()
            }
        ).Take(maxResults).ToListAsync(cancellationToken);

        if (frequentlyBought.Count == 0)
        {
            return Array.Empty<ProductRecommendationDto>();
        }

        var productIds = frequentlyBought.Select(fb => fb.ProductId).ToList();
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        List<ProductRecommendationDto> recommendations = [];
        foreach (var fb in frequentlyBought)
        {
            if (products.TryGetValue(fb.ProductId, out var product))
            {
                var rec = mapper.Map<ProductRecommendationDto>(product);
                recommendations.Add(new ProductRecommendationDto(
                    rec.ProductId,
                    rec.Name,
                    rec.Description,
                    rec.Price,
                    rec.DiscountPrice,
                    rec.ImageUrl,
                    rec.Rating,
                    rec.ReviewCount,
                    "Frequently bought together",
                    fb.Count
                ));
            }
        }

        logger.LogInformation(
            "Frequently bought together tamamlandı. ProductId: {ProductId}, Count: {Count}",
            request.ProductId, recommendations.Count);

        return recommendations;
    }
}
