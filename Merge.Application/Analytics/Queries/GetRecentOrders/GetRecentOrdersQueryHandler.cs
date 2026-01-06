using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetRecentOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetRecentOrdersQueryHandler : IRequestHandler<GetRecentOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetRecentOrdersQueryHandler> _logger;

    public GetRecentOrdersQueryHandler(
        IAdminService adminService,
        ILogger<GetRecentOrdersQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching recent orders. Count: {Count}", request.Count);

        return await _adminService.GetRecentOrdersAsync(request.Count, cancellationToken);
    }
}

