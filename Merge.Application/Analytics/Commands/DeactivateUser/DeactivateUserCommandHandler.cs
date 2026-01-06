using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.DeactivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(
        IAdminService adminService,
        ILogger<DeactivateUserCommandHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user. UserId: {UserId}", request.UserId);

        return await _adminService.DeactivateUserAsync(request.UserId, cancellationToken);
    }
}

