using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, List<TopProductDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetTopProductsQueryHandler> _logger;

    public GetTopProductsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetTopProductsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<TopProductDto>> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top products. StartDate: {StartDate}, EndDate: {EndDate}, Limit: {Limit}",
            request.StartDate, request.EndDate, request.Limit);

        return await _analyticsService.GetTopProductsAsync(request.StartDate, request.EndDate, request.Limit, cancellationToken);
    }
}

