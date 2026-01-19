using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.GetOrganizationRoles;

public record GetOrganizationRolesQuery(
    Guid? OrganizationId = null,
    Guid? UserId = null) : IRequest<List<OrganizationRoleDto>>;
