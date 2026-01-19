using MediatR;
using Merge.Application.DTOs.Identity;

namespace Merge.Application.Identity.Commands.AssignStoreRole;

public record AssignStoreRoleCommand(
    Guid StoreId,
    Guid UserId,
    Guid RoleId,
    Guid? AssignedByUserId = null) : IRequest<StoreRoleDto>;
