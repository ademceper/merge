using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTopCustomersQueryHandler : IRequestHandler<GetTopCustomersQuery, List<TopCustomerDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetTopCustomersQueryHandler> _logger;

    public GetTopCustomersQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetTopCustomersQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<TopCustomerDto>> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top customers. Limit: {Limit}", request.Limit);

        return await _analyticsService.GetTopCustomersAsync(request.Limit, cancellationToken);
    }
}

