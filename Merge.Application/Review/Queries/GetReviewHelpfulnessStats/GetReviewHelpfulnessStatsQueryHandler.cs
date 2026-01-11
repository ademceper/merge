using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Entities.Review;

namespace Merge.Application.Review.Queries.GetReviewHelpfulnessStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReviewHelpfulnessStatsQueryHandler : IRequestHandler<GetReviewHelpfulnessStatsQuery, ReviewHelpfulnessStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetReviewHelpfulnessStatsQueryHandler> _logger;

    public GetReviewHelpfulnessStatsQueryHandler(
        IDbContext context,
        ILogger<GetReviewHelpfulnessStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReviewHelpfulnessStatsDto> Handle(GetReviewHelpfulnessStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Fetching review helpfulness stats. ReviewId: {ReviewId}, UserId: {UserId}",
            request.ReviewId, request.UserId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            _logger.LogWarning("Review not found with Id: {ReviewId}", request.ReviewId);
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        bool? userVote = null;
        if (request.UserId.HasValue)
        {
            // ✅ PERFORMANCE: AsNoTracking for read-only query
            var vote = await _context.Set<ReviewHelpfulness>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rh => rh.ReviewId == request.ReviewId && rh.UserId == request.UserId.Value, cancellationToken);

            if (vote != null)
            {
                userVote = vote.IsHelpful;
            }
        }

        var totalVotes = review.HelpfulCount + review.UnhelpfulCount;
        var helpfulPercentage = totalVotes > 0 ? (decimal)review.HelpfulCount / totalVotes * 100 : 0;

        _logger.LogInformation(
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
