using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.ChangeUserRole;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, bool>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ChangeUserRoleCommandHandler> _logger;

    public ChangeUserRoleCommandHandler(
        IAdminService adminService,
        ILogger<ChangeUserRoleCommandHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Changing user role. UserId: {UserId}, Role: {Role}", request.UserId, request.Role);

        return await _adminService.ChangeUserRoleAsync(request.UserId, request.Role, cancellationToken);
    }
}

