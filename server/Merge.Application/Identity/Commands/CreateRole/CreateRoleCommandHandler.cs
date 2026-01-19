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

namespace Merge.Application.Identity.Commands.CreateRole;

public class CreateRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateRoleCommandHandler> logger) : IRequestHandler<CreateRoleCommand, RoleDto>
{
    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Creating role. Name: {Name}, RoleType: {RoleType}", request.Name, request.RoleType);

        // Check if role already exists
        var existingRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == request.Name, ct);

        if (existingRole is not null)
        {
            throw new Application.Exceptions.BusinessException($"Role '{request.Name}' already exists");
        }

        // Create role
        var role = Role.Create(request.Name, request.RoleType, request.Description, false);

        await context.Roles.AddAsync(role, ct);

        // Add permissions if provided
        if (request.PermissionIds is not null && request.PermissionIds.Count > 0)
        {
            var permissions = await context.Set<Permission>()
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(ct);

            if (permissions.Count != request.PermissionIds.Count)
            {
                throw new Application.Exceptions.NotFoundException("One or more permissions not found");
            }

            var rolePermissions = permissions
                .Select(p => RolePermission.Create(role.Id, p.Id))
                .ToList();

            await context.Set<RolePermission>().AddRangeAsync(rolePermissions, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        // Reload with permissions
        var createdRole = await context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == role.Id, ct);

        return new RoleDto(
            createdRole.Id,
            createdRole.Name ?? string.Empty,
            createdRole.Description,
            createdRole.RoleType,
            createdRole.IsSystemRole,
            createdRole.CreatedAt,
            createdRole.RolePermissions
                .Select(rp => new PermissionDto(
                    rp.Permission.Id,
                    rp.Permission.Name,
                    rp.Permission.Description,
                    rp.Permission.Category,
                    rp.Permission.Resource,
                    rp.Permission.Action,
                    rp.Permission.IsSystemPermission,
                    rp.Permission.CreatedAt))
                .ToList());
    }
}
