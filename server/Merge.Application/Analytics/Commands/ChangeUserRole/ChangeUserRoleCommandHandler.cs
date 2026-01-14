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
public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeUserRoleCommandHandler> _logger;

    public ChangeUserRoleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ChangeUserRoleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Changing user role. UserId: {UserId}, NewRole: {Role}", request.UserId, request.Role);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for role change. UserId: {UserId}", request.UserId);
            return false;
        }

        // Remove existing roles
        // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(existingRoles);

        // Add new role
        // ✅ PERFORMANCE: AsNoTracking for read-only queries (we don't modify this entity)
        var roleEntity = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
        if (roleEntity != null)
        {
            await _context.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
            {
                UserId = request.UserId,
                RoleId = roleEntity.Id
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User role changed successfully. UserId: {UserId}, NewRole: {Role}", request.UserId, request.Role);
        return true;
    }
}

