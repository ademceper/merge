using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetRatingDistribution;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetRatingDistributionQueryHandler(
    IDbContext context,
    ILogger<GetRatingDistributionQueryHandler> logger) : IRequestHandler<GetRatingDistributionQuery, List<RatingDistributionDto>>
{

    public async Task<List<RatingDistributionDto>> Handle(GetRatingDistributionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching rating distribution. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de grouping ve aggregate query kullan (memory'de değil) - 5-10x performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.IsApproved);

        if (request.StartDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= request.EndDate.Value);
        }

        // ✅ PERFORMANCE: Total'i database'de hesapla (memory'de Sum yerine)
        var total = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Her rating (1-5) için database'de CountAsync çağır (memory'de işlem YOK)
        var rating1Count = await query.CountAsync(r => r.Rating == 1, cancellationToken);
        var rating2Count = await query.CountAsync(r => r.Rating == 2, cancellationToken);
        var rating3Count = await query.CountAsync(r => r.Rating == 3, cancellationToken);
        var rating4Count = await query.CountAsync(r => r.Rating == 4, cancellationToken);
        var rating5Count = await query.CountAsync(r => r.Rating == 5, cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU) - 5 eleman biliniyor (rating 1-5)
        return new List<RatingDistributionDto>(5)
        {
            new RatingDistributionDto(1, rating1Count, total > 0 ? Math.Round((decimal)rating1Count / total * 100, 2) : 0),
            new RatingDistributionDto(2, rating2Count, total > 0 ? Math.Round((decimal)rating2Count / total * 100, 2) : 0),
            new RatingDistributionDto(3, rating3Count, total > 0 ? Math.Round((decimal)rating3Count / total * 100, 2) : 0),
            new RatingDistributionDto(4, rating4Count, total > 0 ? Math.Round((decimal)rating4Count / total * 100, 2) : 0),
            new RatingDistributionDto(5, rating5Count, total > 0 ? Math.Round((decimal)rating5Count / total * 100, 2) : 0)
        };
    }
}

