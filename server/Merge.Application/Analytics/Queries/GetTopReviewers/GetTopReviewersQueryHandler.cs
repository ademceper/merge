using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

namespace Merge.Application.Analytics.Queries.GetTopReviewers;

public class GetTopReviewersQueryHandler(
    IDbContext context,
    ILogger<GetTopReviewersQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetTopReviewersQuery, List<ReviewerStatsDto>>
{

    public async Task<List<ReviewerStatsDto>> Handle(GetTopReviewersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top reviewers. Limit: {Limit}", request.Limit);

        var limit = request.Limit == 10 ? settings.Value.TopProductsLimit : request.Limit;
        
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

