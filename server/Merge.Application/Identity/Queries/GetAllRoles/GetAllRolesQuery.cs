using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Domain.Enums;

namespace Merge.Application.Identity.Queries.GetAllRoles;

public record GetAllRolesQuery(
    RoleType? RoleType = null) : IRequest<List<RoleDto>>;
