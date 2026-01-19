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

namespace Merge.Application.Identity.Commands.AssignOrganizationRole;

public class AssignOrganizationRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AssignOrganizationRoleCommandHandler> logger) : IRequestHandler<AssignOrganizationRoleCommand, OrganizationRoleDto>
{
    public async Task<OrganizationRoleDto> Handle(AssignOrganizationRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Assigning organization role. OrganizationId: {OrganizationId}, UserId: {UserId}, RoleId: {RoleId}", 
            request.OrganizationId, request.UserId, request.RoleId);

        // Check if organization exists
        var organization = await context.Set<Merge.Domain.Modules.Identity.Organization>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, ct);

        if (organization is null)
        {
            throw new Application.Exceptions.NotFoundException("Organization", request.OrganizationId);
        }

        // Check if user exists
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null)
        {
            throw new Application.Exceptions.NotFoundException("User", request.UserId);
        }

        // Check if role exists and is Organization type
        var role = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);

        if (role is null)
        {
            throw new Application.Exceptions.NotFoundException("Role", request.RoleId);
        }

        if (role.RoleType != RoleType.Organization)
        {
            throw new Application.Exceptions.BusinessException("Role must be of type Organization");
        }

        // Check if already assigned
        var existing = await context.Set<OrganizationRole>()
            .AsNoTracking()
            .FirstOrDefaultAsync(or => or.OrganizationId == request.OrganizationId && 
                                      or.UserId == request.UserId && 
                                      or.RoleId == request.RoleId && 
                                      !or.IsDeleted, ct);

        if (existing is not null)
        {
            throw new Application.Exceptions.BusinessException("Organization role already assigned");
        }

        // Create organization role
        var organizationRole = OrganizationRole.Create(
            request.OrganizationId,
            request.UserId,
            request.RoleId,
            request.AssignedByUserId);

        await context.Set<OrganizationRole>().AddAsync(organizationRole, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var created = await context.Set<OrganizationRole>()
            .Include(or => or.Organization)
            .Include(or => or.User)
            .Include(or => or.Role)
            .FirstAsync(or => or.Id == organizationRole.Id, ct);

        return new OrganizationRoleDto(
            created.Id,
            created.OrganizationId,
            created.Organization.Name,
            created.UserId,
            created.User.Email ?? string.Empty,
            created.RoleId,
            created.Role.Name ?? string.Empty,
            created.AssignedAt,
            created.AssignedByUserId);
    }
}
