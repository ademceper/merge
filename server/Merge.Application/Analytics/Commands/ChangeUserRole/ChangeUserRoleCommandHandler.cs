using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.ChangeUserRole;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ChangeUserRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ChangeUserRoleCommandHandler> logger) : IRequestHandler<ChangeUserRoleCommand, bool>
{

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing user role. UserId: {UserId}, NewRole: {Role}", request.UserId, request.Role);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found for role change. UserId: {UserId}", request.UserId);
            return false;
        }

        // Remove existing roles
        // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
        var existingRoles = await context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        context.UserRoles.RemoveRange(existingRoles);

        // Add new role
        // ✅ PERFORMANCE: AsNoTracking for read-only queries (we don't modify this entity)
        var roleEntity = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
        if (roleEntity != null)
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

