using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using UserEntity = Merge.Domain.Modules.Identity.User;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Identity.Queries.GetUserRolesAndPermissions;

public class GetUserRolesAndPermissionsQueryHandler(
    IDbContext context,
    UserManager<UserEntity> userManager,
    ILogger<GetUserRolesAndPermissionsQueryHandler> logger) : IRequestHandler<GetUserRolesAndPermissionsQuery, UserRolesAndPermissionsDto>
{
    public async Task<UserRolesAndPermissionsDto> Handle(GetUserRolesAndPermissionsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Getting roles and permissions for user {UserId}", request.UserId);

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Platform roles
        var platformRoles = await userManager.GetRolesAsync(user);

        // Store roles
        var storeRoles = await context.Set<StoreRole>()
            .AsNoTracking()
            .Include(sr => sr.Store)
            .Include(sr => sr.Role)
            .Where(sr => sr.UserId == request.UserId && !sr.IsDeleted)
            .Select(sr => new StoreRoleInfo(
                sr.StoreId,
                sr.Store.StoreName,
                sr.Role.Name ?? string.Empty,
                sr.RoleId))
            .ToListAsync(ct);

        // Organization roles
        var organizationRoles = await context.Set<OrganizationRole>()
            .AsNoTracking()
            .Include(or => or.Organization)
            .Include(or => or.Role)
            .Where(or => or.UserId == request.UserId && !or.IsDeleted)
            .Select(or => new OrganizationRoleInfo(
                or.OrganizationId,
                or.Organization.Name,
                or.Role.Name ?? string.Empty,
                or.RoleId))
            .ToListAsync(ct);

        // Store customer roles
        var storeCustomerRoles = await context.Set<StoreCustomerRole>()
            .AsNoTracking()
            .Include(scr => scr.Store)
            .Include(scr => scr.Role)
            .Where(scr => scr.UserId == request.UserId && !scr.IsDeleted)
            .Select(scr => new StoreCustomerRoleInfo(
                scr.StoreId,
                scr.Store.StoreName,
                scr.Role.Name ?? string.Empty,
                scr.RoleId))
            .ToListAsync(ct);

        // Get all permissions from all roles
        var roleIds = new List<Guid>();

        // Platform role IDs
        var platformRoleEntities = await context.Roles
            .AsNoTracking()
            .Where(r => platformRoles.Contains(r.Name ?? string.Empty))
            .Select(r => r.Id)
            .ToListAsync(ct);
        roleIds.AddRange(platformRoleEntities);

        // Store role IDs
        roleIds.AddRange(storeRoles.Select(sr => sr.RoleId));

        // Organization role IDs
        roleIds.AddRange(organizationRoles.Select(or => or.RoleId));

        // Store customer role IDs
        roleIds.AddRange(storeCustomerRoles.Select(scr => scr.RoleId));

        // Get unique permissions
        var permissions = await context.Set<RolePermission>()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(ct);

        return new UserRolesAndPermissionsDto(
            PlatformRoles: platformRoles.ToList(),
            StoreRoles: storeRoles,
            OrganizationRoles: organizationRoles,
            StoreCustomerRoles: storeCustomerRoles,
            Permissions: permissions);
    }
}
