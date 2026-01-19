using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Identity.Queries.GetAllPermissions;

public class GetAllPermissionsQueryHandler(
    IDbContext context,
    ILogger<GetAllPermissionsQueryHandler> logger) : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Getting all permissions. Category: {Category}, Resource: {Resource}", request.Category, request.Resource);

        var query = context.Set<Permission>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        if (!string.IsNullOrEmpty(request.Resource))
        {
            query = query.Where(p => p.Resource == request.Resource);
        }

        var permissions = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionDto(
                p.Id,
                p.Name,
                p.Description,
                p.Category,
                p.Resource,
                p.Action,
                p.IsSystemPermission,
                p.CreatedAt))
            .ToListAsync(ct);

        return permissions;
    }
}
