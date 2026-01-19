using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Identity.Queries.GetAllRoles;

public class GetAllRolesQueryHandler(
    IDbContext context,
    ILogger<GetAllRolesQueryHandler> logger) : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Getting all roles. RoleType: {RoleType}", request.RoleType);

        var query = context.Roles
            .AsNoTracking()
            .AsQueryable();

        if (request.RoleType.HasValue)
        {
            query = query.Where(r => r.RoleType == request.RoleType.Value);
        }

        var roles = await query
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.RoleType)
            .ThenBy(r => r.Name)
            .ToListAsync(ct);

        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name ?? string.Empty,
            r.Description,
            r.RoleType,
            r.IsSystemRole,
            r.CreatedAt,
            r.RolePermissions
                .Select(rp => new PermissionDto(
                    rp.Permission.Id,
                    rp.Permission.Name,
                    rp.Permission.Description,
                    rp.Permission.Category,
                    rp.Permission.Resource,
                    rp.Permission.Action,
                    rp.Permission.IsSystemPermission,
                    rp.Permission.CreatedAt))
                .ToList()))
            .ToList();
    }
}
