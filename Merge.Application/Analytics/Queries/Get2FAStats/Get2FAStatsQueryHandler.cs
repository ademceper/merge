using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.Get2FAStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class Get2FAStatsQueryHandler : IRequestHandler<Get2FAStatsQuery, TwoFactorStatsDto>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<Get2FAStatsQueryHandler> _logger;

    public Get2FAStatsQueryHandler(
        IAdminService adminService,
        ILogger<Get2FAStatsQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<TwoFactorStatsDto> Handle(Get2FAStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching 2FA stats");

        return await _adminService.Get2FAStatsAsync(cancellationToken);
    }
}

