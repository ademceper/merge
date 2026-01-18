using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetReviewAnalytics;

public class GetReviewAnalyticsQueryHandler(
    IDbContext context,
    ILogger<GetReviewAnalyticsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetReviewAnalyticsQuery, ReviewAnalyticsDto>
{

    public async Task<ReviewAnalyticsDto> Handle(GetReviewAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching review analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var reviewsQuery = context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate);

        // Database'de aggregateler
        var totalReviews = await reviewsQuery.CountAsync(cancellationToken);
        var approvedReviews = await reviewsQuery.CountAsync(r => r.IsApproved, cancellationToken);
        var pendingReviews = await reviewsQuery.CountAsync(r => !r.IsApproved, cancellationToken);
        var rejectedReviews = 0; // Deleted reviews are filtered out by Global Query Filter
        var averageRating = await reviewsQuery.AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0;
        var verifiedPurchaseReviews = await reviewsQuery.CountAsync(r => r.IsVerifiedPurchase, cancellationToken);
        
        // Helpful votes - Database'de aggregate
        var helpfulVotes = await reviewsQuery.SumAsync(r => r.HelpfulCount, cancellationToken);
        var unhelpfulVotes = await reviewsQuery.SumAsync(r => r.UnhelpfulCount, cancellationToken);
        var totalVotes = helpfulVotes + unhelpfulVotes;
        var helpfulPercentage = totalVotes > 0 ? (decimal)helpfulVotes / totalVotes * 100 : 0;

        // Reviews with media - Database'de count
        var reviewIdsSubquery = from r in reviewsQuery select r.Id;
        var reviewsWithMedia = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(rm => reviewIdsSubquery.Contains(rm.ReviewId))
            .Select(rm => rm.ReviewId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new ReviewAnalyticsDto(
            request.StartDate,
            request.EndDate,
            totalReviews,
            approvedReviews,
            pendingReviews,
            rejectedReviews,
            Math.Round(averageRating, 2),
            reviewsWithMedia,
            verifiedPurchaseReviews,
            Math.Round(helpfulPercentage, 2),
            await GetRatingDistributionAsync(request.StartDate, request.EndDate, cancellationToken),
            await GetReviewTrendsAsync(request.StartDate, request.EndDate, cancellationToken),
            await GetTopReviewedProductsAsync(settings.Value.MaxQueryLimit, cancellationToken),
            await GetTopReviewersAsync(settings.Value.MaxQueryLimit, cancellationToken)
        );
    }

    private async Task<List<RatingDistributionDto>> GetRatingDistributionAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var query = context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.IsApproved);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= endDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rating1Count = await query.CountAsync(r => r.Rating == 1, cancellationToken);
        var rating2Count = await query.CountAsync(r => r.Rating == 2, cancellationToken);
        var rating3Count = await query.CountAsync(r => r.Rating == 3, cancellationToken);
        var rating4Count = await query.CountAsync(r => r.Rating == 4, cancellationToken);
        var rating5Count = await query.CountAsync(r => r.Rating == 5, cancellationToken);

        return new List<RatingDistributionDto>(5)
        {
            new RatingDistributionDto(1, rating1Count, total > 0 ? Math.Round((decimal)rating1Count / total * 100, 2) : 0),
            new RatingDistributionDto(2, rating2Count, total > 0 ? Math.Round((decimal)rating2Count / total * 100, 2) : 0),
            new RatingDistributionDto(3, rating3Count, total > 0 ? Math.Round((decimal)rating3Count / total * 100, 2) : 0),
            new RatingDistributionDto(4, rating4Count, total > 0 ? Math.Round((decimal)rating4Count / total * 100, 2) : 0),
            new RatingDistributionDto(5, rating5Count, total > 0 ? Math.Round((decimal)rating5Count / total * 100, 2) : 0)
        };
    }

    private async Task<List<ReviewTrendDto>> GetReviewTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.IsApproved && r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new ReviewTrendDto(
                g.Key,
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2)
            ))
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<TopReviewedProductDto>> GetTopReviewedProductsAsync(int limit, CancellationToken cancellationToken)
    {
        return await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved)
            .GroupBy(r => new { r.ProductId, ProductName = r.Product.Name })
            .Select(g => new TopReviewedProductDto(
                g.Key.ProductId,
                g.Key.ProductName ?? string.Empty,
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2),
                g.Sum(r => r.HelpfulCount)
            ))
            .OrderByDescending(p => p.ReviewCount)
            .ThenByDescending(p => p.AverageRating)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ReviewerStatsDto>> GetTopReviewersAsync(int limit, CancellationToken cancellationToken)
    {
        return await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.IsApproved)
            .GroupBy(r => new { r.UserId, r.User.FirstName, r.User.LastName })
            .Select(g => new ReviewerStatsDto(
                g.Key.UserId,
                $"{g.Key.FirstName} {g.Key.LastName}",
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2),
                g.Sum(r => r.HelpfulCount)
            ))
            .OrderByDescending(r => r.ReviewCount)
            .ThenByDescending(r => r.HelpfulVotes)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

