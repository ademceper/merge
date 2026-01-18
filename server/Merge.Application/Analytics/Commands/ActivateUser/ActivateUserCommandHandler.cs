using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.ActivateUser;

public class ActivateUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ActivateUserCommandHandler> logger) : IRequestHandler<ActivateUserCommand, bool>
{

    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Activating user. UserId: {UserId}", request.UserId);
        
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("User not found for activation. UserId: {UserId}", request.UserId);
            return false;
        }

        user.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User activated successfully. UserId: {UserId}", request.UserId);
        return true;
    }
}

