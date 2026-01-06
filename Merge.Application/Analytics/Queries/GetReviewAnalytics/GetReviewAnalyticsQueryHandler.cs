using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetReviewAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReviewAnalyticsQueryHandler : IRequestHandler<GetReviewAnalyticsQuery, ReviewAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetReviewAnalyticsQueryHandler> _logger;

    public GetReviewAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetReviewAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<ReviewAnalyticsDto> Handle(GetReviewAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching review analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetReviewAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

