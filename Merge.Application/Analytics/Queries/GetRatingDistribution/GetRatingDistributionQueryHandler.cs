using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetRatingDistribution;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetRatingDistributionQueryHandler : IRequestHandler<GetRatingDistributionQuery, List<RatingDistributionDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetRatingDistributionQueryHandler> _logger;

    public GetRatingDistributionQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetRatingDistributionQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<RatingDistributionDto>> Handle(GetRatingDistributionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching rating distribution. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetRatingDistributionAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

