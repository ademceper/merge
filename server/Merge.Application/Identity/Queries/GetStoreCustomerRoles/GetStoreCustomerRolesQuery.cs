using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Queries.GetStoreCustomerRoles;

public record GetStoreCustomerRolesQuery(
    Guid? StoreId = null,
    Guid? UserId = null) : IRequest<List<StoreCustomerRoleDto>>;
