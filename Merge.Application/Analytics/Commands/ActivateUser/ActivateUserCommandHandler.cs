using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.ActivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IAdminService adminService,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating user. UserId: {UserId}", request.UserId);

        return await _adminService.ActivateUserAsync(request.UserId, cancellationToken);
    }
}

