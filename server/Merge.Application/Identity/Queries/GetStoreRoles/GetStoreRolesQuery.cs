using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.GetStoreRoles;

public record GetStoreRolesQuery(
    Guid? StoreId = null,
    Guid? UserId = null) : IRequest<List<StoreRoleDto>>;
