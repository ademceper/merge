using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetReviewHelpfulnessStats;

public class GetReviewHelpfulnessStatsQueryHandler(IDbContext context, ILogger<GetReviewHelpfulnessStatsQueryHandler> logger) : IRequestHandler<GetReviewHelpfulnessStatsQuery, ReviewHelpfulnessStatsDto>
{

    public async Task<ReviewHelpfulnessStatsDto> Handle(GetReviewHelpfulnessStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching review helpfulness stats. ReviewId: {ReviewId}, UserId: {UserId}",
            request.ReviewId, request.UserId);

        var review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            logger.LogWarning("Review not found with Id: {ReviewId}", request.ReviewId);
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        bool? userVote = null;
        if (request.UserId.HasValue)
        {
            var vote = await context.Set<ReviewHelpfulness>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rh => rh.ReviewId == request.ReviewId && rh.UserId == request.UserId.Value, cancellationToken);

            if (vote != null)
            {
                userVote = vote.IsHelpful;
            }
        }

        var totalVotes = review.HelpfulCount + review.UnhelpfulCount;
        var helpfulPercentage = totalVotes > 0 ? (decimal)review.HelpfulCount / totalVotes * 100 : 0;

        logger.LogInformation(
            "Review helpfulness stats retrieved. ReviewId: {ReviewId}, HelpfulCount: {HelpfulCount}, UnhelpfulCount: {UnhelpfulCount}, TotalVotes: {TotalVotes}",
            request.ReviewId, review.HelpfulCount, review.UnhelpfulCount, totalVotes);

        return new ReviewHelpfulnessStatsDto
        {
            ReviewId = request.ReviewId,
            HelpfulCount = review.HelpfulCount,
            UnhelpfulCount = review.UnhelpfulCount,
            TotalVotes = totalVotes,
            HelpfulPercentage = Math.Round(helpfulPercentage, 2),
            UserVote = userVote
        };
    }
}
