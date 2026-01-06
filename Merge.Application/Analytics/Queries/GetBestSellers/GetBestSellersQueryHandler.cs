using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetBestSellers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetBestSellersQueryHandler : IRequestHandler<GetBestSellersQuery, List<TopProductDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetBestSellersQueryHandler> _logger;

    public GetBestSellersQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetBestSellersQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<TopProductDto>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching best sellers. Limit: {Limit}", request.Limit);

        return await _analyticsService.GetBestSellersAsync(request.Limit, cancellationToken);
    }
}

