using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Enums;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.AssignStoreRole;

public class AssignStoreRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AssignStoreRoleCommandHandler> logger) : IRequestHandler<AssignStoreRoleCommand, StoreRoleDto>
{
    public async Task<StoreRoleDto> Handle(AssignStoreRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Assigning store role. StoreId: {StoreId}, UserId: {UserId}, RoleId: {RoleId}", 
            request.StoreId, request.UserId, request.RoleId);

        // Check if store exists
        var store = await context.Set<Merge.Domain.Modules.Marketplace.Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, ct);

        if (store is null)
        {
            throw new Application.Exceptions.NotFoundException("Store", request.StoreId);
        }

        // Check if user exists
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null)
        {
            throw new Application.Exceptions.NotFoundException("User", request.UserId);
        }

        // Check if role exists and is Store type
        var role = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);

        if (role is null)
        {
            throw new Application.Exceptions.NotFoundException("Role", request.RoleId);
        }

        if (role.RoleType != RoleType.Store)
        {
            throw new Application.Exceptions.BusinessException("Role must be of type Store");
        }

        // Check if already assigned
        var existing = await context.Set<StoreRole>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.StoreId == request.StoreId && 
                                      sr.UserId == request.UserId && 
                                      sr.RoleId == request.RoleId && 
                                      !sr.IsDeleted, ct);

        if (existing is not null)
        {
            throw new Application.Exceptions.BusinessException("Store role already assigned");
        }

        // Create store role
        var storeRole = StoreRole.Create(
            request.StoreId,
            request.UserId,
            request.RoleId,
            request.AssignedByUserId);

        await context.Set<StoreRole>().AddAsync(storeRole, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var created = await context.Set<StoreRole>()
            .Include(sr => sr.Store)
            .Include(sr => sr.User)
            .Include(sr => sr.Role)
            .FirstAsync(sr => sr.Id == storeRole.Id, ct);

        return new StoreRoleDto(
            created.Id,
            created.StoreId,
            created.Store.StoreName,
            created.UserId,
            created.User.Email ?? string.Empty,
            created.RoleId,
            created.Role.Name ?? string.Empty,
            created.AssignedAt,
            created.AssignedByUserId);
    }
}
