using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetWorstPerformers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetWorstPerformersQueryHandler : IRequestHandler<GetWorstPerformersQuery, List<TopProductDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetWorstPerformersQueryHandler> _logger;

    public GetWorstPerformersQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetWorstPerformersQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<TopProductDto>> Handle(GetWorstPerformersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching worst performers. Limit: {Limit}", request.Limit);

        return await _analyticsService.GetWorstPerformersAsync(request.Limit, cancellationToken);
    }
}

