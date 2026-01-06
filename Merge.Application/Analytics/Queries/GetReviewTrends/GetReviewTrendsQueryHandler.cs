using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetReviewTrends;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReviewTrendsQueryHandler : IRequestHandler<GetReviewTrendsQuery, List<ReviewTrendDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetReviewTrendsQueryHandler> _logger;

    public GetReviewTrendsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetReviewTrendsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<ReviewTrendDto>> Handle(GetReviewTrendsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching review trends. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetReviewTrendsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

