using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetMostHelpfulReviews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetMostHelpfulReviewsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetMostHelpfulReviewsQueryHandler> logger, IOptions<ReviewSettings> reviewSettings) : IRequestHandler<GetMostHelpfulReviewsQuery, IEnumerable<ReviewHelpfulnessStatsDto>>
{
    private readonly ReviewSettings reviewConfig = reviewSettings.Value;

    public async Task<IEnumerable<ReviewHelpfulnessStatsDto>> Handle(GetMostHelpfulReviewsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var limit = request.Limit > reviewConfig.MaxHelpfulReviewsLimit
            ? reviewConfig.MaxHelpfulReviewsLimit
            : request.Limit;
        if (limit < 1) limit = reviewConfig.DefaultHelpfulReviewsLimit;

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Fetching most helpful reviews. ProductId: {ProductId}, Limit: {Limit}",
            request.ProductId, limit);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var reviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.ProductId == request.ProductId && r.IsApproved)
            .OrderByDescending(r => r.HelpfulCount)
            .ThenByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved {Count} most helpful reviews for product {ProductId}",
            reviews.Count, request.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var stats = mapper.Map<IEnumerable<ReviewHelpfulnessStatsDto>>(reviews).ToList();
        foreach (var stat in stats)
        {
            stat.UserVote = null; // GetMostHelpfulReviews için user vote null
        }
        return stats;
    }
}
