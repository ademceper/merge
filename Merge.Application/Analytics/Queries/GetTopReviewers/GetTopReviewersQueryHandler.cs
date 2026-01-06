using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopReviewers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTopReviewersQueryHandler : IRequestHandler<GetTopReviewersQuery, List<ReviewerStatsDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetTopReviewersQueryHandler> _logger;

    public GetTopReviewersQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetTopReviewersQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<ReviewerStatsDto>> Handle(GetTopReviewersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top reviewers. Limit: {Limit}", request.Limit);

        return await _analyticsService.GetTopReviewersAsync(request.Limit, cancellationToken);
    }
}

