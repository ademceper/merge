using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetCouponPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCouponPerformanceQueryHandler : IRequestHandler<GetCouponPerformanceQuery, List<CouponPerformanceDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetCouponPerformanceQueryHandler> _logger;

    public GetCouponPerformanceQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetCouponPerformanceQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<CouponPerformanceDto>> Handle(GetCouponPerformanceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching coupon performance. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetCouponPerformanceAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

