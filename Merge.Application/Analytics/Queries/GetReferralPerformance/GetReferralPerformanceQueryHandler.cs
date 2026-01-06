using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetReferralPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReferralPerformanceQueryHandler : IRequestHandler<GetReferralPerformanceQuery, ReferralPerformanceDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetReferralPerformanceQueryHandler> _logger;

    public GetReferralPerformanceQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetReferralPerformanceQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<ReferralPerformanceDto> Handle(GetReferralPerformanceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching referral performance. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetReferralPerformanceAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

