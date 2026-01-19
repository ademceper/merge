using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Identity.Queries.GetOrganizationRoles;

public class GetOrganizationRolesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetOrganizationRolesQueryHandler> logger) : IRequestHandler<GetOrganizationRolesQuery, List<OrganizationRoleDto>>
{
    public async Task<List<OrganizationRoleDto>> Handle(GetOrganizationRolesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Getting organization roles. OrganizationId: {OrganizationId}, UserId: {UserId}", 
            request.OrganizationId, request.UserId);

        var query = context.Set<OrganizationRole>()
            .AsNoTracking()
            .Include(or => or.Organization)
            .Include(or => or.User)
            .Include(or => or.Role)
            .Where(or => !or.IsDeleted)
            .AsQueryable();

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(or => or.OrganizationId == request.OrganizationId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(or => or.UserId == request.UserId.Value);
        }

        var organizationRoles = await query
            .OrderBy(or => or.Organization.Name)
            .ThenBy(or => or.Role.Name)
            .Select(or => new OrganizationRoleDto(
                or.Id,
                or.OrganizationId,
                or.Organization.Name,
                or.UserId,
                or.User.Email ?? string.Empty,
                or.RoleId,
                or.Role.Name ?? string.Empty,
                or.AssignedAt,
                or.AssignedByUserId))
            .ToListAsync(ct);

        return organizationRoles;
    }
}
