using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetAdminTopProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAdminTopProductsQueryHandler : IRequestHandler<GetAdminTopProductsQuery, IEnumerable<AdminTopProductDto>>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetAdminTopProductsQueryHandler> _logger;

    public GetAdminTopProductsQueryHandler(
        IAdminService adminService,
        ILogger<GetAdminTopProductsQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IEnumerable<AdminTopProductDto>> Handle(GetAdminTopProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching admin top products. Count: {Count}", request.Count);

        return await _adminService.GetTopProductsAsync(request.Count, cancellationToken);
    }
}

