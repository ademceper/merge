using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.AssignOrganizationRole;

public record AssignOrganizationRoleCommand(
    Guid OrganizationId,
    Guid UserId,
    Guid RoleId,
    Guid? AssignedByUserId = null) : IRequest<OrganizationRoleDto>;
