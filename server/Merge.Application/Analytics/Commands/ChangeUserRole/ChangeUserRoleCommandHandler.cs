using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ChangeUserRoleCommandHandler> logger) : IRequestHandler<ChangeUserRoleCommand, bool>
{

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing user role. UserId: {UserId}, NewRole: {Role}", request.UserId, request.Role);
        
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("User not found for role change. UserId: {UserId}", request.UserId);
            return false;
        }

        // Remove existing roles
        var existingRoles = await context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        context.UserRoles.RemoveRange(existingRoles);

        // Add new role
        var roleEntity = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
        if (roleEntity is not null)
        {
            await context.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
            {
                UserId = request.UserId,
                RoleId = roleEntity.Id
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User role changed successfully. UserId: {UserId}, NewRole: {Role}", request.UserId, request.Role);
        return true;
    }
}

