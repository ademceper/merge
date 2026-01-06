using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetSalesByCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSalesByCategoryQueryHandler : IRequestHandler<GetSalesByCategoryQuery, List<CategorySalesDto>>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetSalesByCategoryQueryHandler> _logger;

    public GetSalesByCategoryQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetSalesByCategoryQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<List<CategorySalesDto>> Handle(GetSalesByCategoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching sales by category. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await _analyticsService.GetSalesByCategoryAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}

