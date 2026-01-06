using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerSegments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCustomerSegmentsQueryHandler : IRequestHandler<GetCustomerSegmentsQuery, List<CustomerSegmentDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetCustomerSegmentsQueryHandler> _logger;

    public GetCustomerSegmentsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetCustomerSegmentsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<CustomerSegmentDto>> Handle(GetCustomerSegmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching customer segments");

        return await _analyticsService.GetCustomerSegmentsAsync(cancellationToken);
    }
}

