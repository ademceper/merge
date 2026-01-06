using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCustomerAnalyticsQueryHandler : IRequestHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetCustomerAnalyticsQueryHandler> _logger;

    public GetCustomerAnalyticsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetCustomerAnalyticsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<CustomerAnalyticsDto> Handle(GetCustomerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching customer analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetCustomerAnalyticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

