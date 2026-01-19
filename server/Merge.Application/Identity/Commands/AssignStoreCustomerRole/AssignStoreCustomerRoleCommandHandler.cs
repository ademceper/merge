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

namespace Merge.Application.Identity.Commands.AssignStoreCustomerRole;

public class AssignStoreCustomerRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AssignStoreCustomerRoleCommandHandler> logger) : IRequestHandler<AssignStoreCustomerRoleCommand, StoreCustomerRoleDto>
{
    public async Task<StoreCustomerRoleDto> Handle(AssignStoreCustomerRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Assigning store customer role. StoreId: {StoreId}, UserId: {UserId}, RoleId: {RoleId}", 
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

        // Check if role exists and is StoreCustomer type
        var role = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);

        if (role is null)
        {
            throw new Application.Exceptions.NotFoundException("Role", request.RoleId);
        }

        if (role.RoleType != RoleType.StoreCustomer)
        {
            throw new Application.Exceptions.BusinessException("Role must be of type StoreCustomer");
        }

        // Check if already assigned
        var existing = await context.Set<StoreCustomerRole>()
            .AsNoTracking()
            .FirstOrDefaultAsync(scr => scr.StoreId == request.StoreId && 
                                       scr.UserId == request.UserId && 
                                       scr.RoleId == request.RoleId && 
                                       !scr.IsDeleted, ct);

        if (existing is not null)
        {
            throw new Application.Exceptions.BusinessException("Store customer role already assigned");
        }

        // Create store customer role
        var storeCustomerRole = StoreCustomerRole.Create(
            request.StoreId,
            request.UserId,
            request.RoleId,
            request.AssignedByUserId);

        await context.Set<StoreCustomerRole>().AddAsync(storeCustomerRole, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var created = await context.Set<StoreCustomerRole>()
            .Include(scr => scr.Store)
            .Include(scr => scr.User)
            .Include(scr => scr.Role)
            .FirstAsync(scr => scr.Id == storeCustomerRole.Id, ct);

        return new StoreCustomerRoleDto(
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
