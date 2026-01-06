using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopReviewedProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTopReviewedProductsQueryHandler : IRequestHandler<GetTopReviewedProductsQuery, List<TopReviewedProductDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetTopReviewedProductsQueryHandler> _logger;

    public GetTopReviewedProductsQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetTopReviewedProductsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<TopReviewedProductDto>> Handle(GetTopReviewedProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top reviewed products. Limit: {Limit}", request.Limit);

        return await _analyticsService.GetTopReviewedProductsAsync(request.Limit, cancellationToken);
    }
}

