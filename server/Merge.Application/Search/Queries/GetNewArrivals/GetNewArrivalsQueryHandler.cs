using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetNewArrivals;

public class GetNewArrivalsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetNewArrivalsQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetNewArrivalsQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetNewArrivalsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "New arrivals isteniyor. Days: {Days}, MaxResults: {MaxResults}",
            request.Days, request.MaxResults);

        var days = request.Days < 1 ? searchConfig.DefaultNewArrivalsDays : request.Days;
        if (days > searchConfig.MaxTrendingDays) days = searchConfig.MaxTrendingDays;

        var maxResults = request.MaxResults > searchConfig.MaxRecommendationResults
            ? searchConfig.MaxRecommendationResults
            : request.MaxResults;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var newArrivals = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive && p.CreatedAt >= startDate)
            .OrderByDescending(p => p.CreatedAt)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        var recommendations = mapper.Map<IEnumerable<ProductRecommendationDto>>(newArrivals)
            .Select(rec => new ProductRecommendationDto(
                rec.ProductId,
                rec.Name,
                rec.Description,
                rec.Price,
                rec.DiscountPrice,
                rec.ImageUrl,
                rec.Rating,
                rec.ReviewCount,
                "New arrival",
                0
            ))
            .ToList();

        logger.LogInformation(
            "New arrivals tamamlandı. Days: {Days}, Count: {Count}",
            days, recommendations.Count);

        return recommendations;
    }
}
