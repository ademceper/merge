using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Search.Queries.GetNewArrivals;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetNewArrivalsQueryHandler : IRequestHandler<GetNewArrivalsQuery, IReadOnlyList<ProductRecommendationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetNewArrivalsQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetNewArrivalsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetNewArrivalsQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<IReadOnlyList<ProductRecommendationDto>> Handle(GetNewArrivalsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "New arrivals isteniyor. Days: {Days}, MaxResults: {MaxResults}",
            request.Days, request.MaxResults);

        var days = request.Days < 1 ? _searchSettings.DefaultNewArrivalsDays : request.Days;
        if (days > _searchSettings.MaxTrendingDays) days = _searchSettings.MaxTrendingDays;

        var maxResults = request.MaxResults > _searchSettings.MaxRecommendationResults
            ? _searchSettings.MaxRecommendationResults
            : request.MaxResults;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var newArrivals = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive && p.CreatedAt >= startDate)
            .OrderByDescending(p => p.CreatedAt)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recommendations = _mapper.Map<IEnumerable<ProductRecommendationDto>>(newArrivals)
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

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "New arrivals tamamlandı. Days: {Days}, Count: {Count}",
            days, recommendations.Count);

        return recommendations;
    }
}
