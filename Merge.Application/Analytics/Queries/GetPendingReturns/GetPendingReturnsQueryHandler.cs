using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPendingReturnsQueryHandler : IRequestHandler<GetPendingReturnsQuery, PagedResult<ReturnRequestDto>>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetPendingReturnsQueryHandler> _logger;

    public GetPendingReturnsQueryHandler(
        IAdminService adminService,
        ILogger<GetPendingReturnsQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<PagedResult<ReturnRequestDto>> Handle(GetPendingReturnsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching pending returns. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        return await _adminService.GetPendingReturnsAsync(request.Page, request.PageSize, cancellationToken);
    }
}

