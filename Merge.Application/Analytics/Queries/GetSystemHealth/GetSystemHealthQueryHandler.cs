using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetSystemHealth;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSystemHealthQueryHandler : IRequestHandler<GetSystemHealthQuery, SystemHealthDto>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetSystemHealthQueryHandler> _logger;

    public GetSystemHealthQueryHandler(
        IAdminService adminService,
        ILogger<GetSystemHealthQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<SystemHealthDto> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching system health");

        return await _adminService.GetSystemHealthAsync(cancellationToken);
    }
}

