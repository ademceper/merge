using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Domain.Enums;

namespace Merge.Application.Identity.Commands.CreateRole;

public record CreateRoleCommand(
    string Name,
    RoleType RoleType,
    string? Description = null,
    List<Guid>? PermissionIds = null) : IRequest<RoleDto>;
