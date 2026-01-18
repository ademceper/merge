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

namespace Merge.Application.Analytics.Queries.GetReviewTrends;

public class GetReviewTrendsQueryHandler(
    IDbContext context,
    ILogger<GetReviewTrendsQueryHandler> logger) : IRequestHandler<GetReviewTrendsQuery, List<ReviewTrendDto>>
{

    public async Task<List<ReviewTrendDto>> Handle(GetReviewTrendsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching review trends. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.IsApproved && r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new ReviewTrendDto(
                g.Key,
                g.Count(),
                Math.Round((decimal)g.Average(r => r.Rating), 2)
            ))
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);
    }
}

