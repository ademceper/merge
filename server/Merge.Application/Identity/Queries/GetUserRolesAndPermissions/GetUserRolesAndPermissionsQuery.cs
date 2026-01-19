using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.GetUserRolesAndPermissions;

public record GetUserRolesAndPermissionsQuery(Guid UserId) : IRequest<UserRolesAndPermissionsDto>;
