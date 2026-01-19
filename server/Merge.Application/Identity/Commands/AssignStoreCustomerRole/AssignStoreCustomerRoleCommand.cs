using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.AssignStoreCustomerRole;

public record AssignStoreCustomerRoleCommand(
    Guid StoreId,
    Guid UserId,
    Guid RoleId,
    Guid? AssignedByUserId = null) : IRequest<StoreCustomerRoleDto>;
