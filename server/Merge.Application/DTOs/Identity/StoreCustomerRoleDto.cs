namespace Merge.Application.DTOs.Identity;

public record StoreCustomerRoleDto(
    Guid Id,
    Guid StoreId,
    string StoreName,
    Guid UserId,
    string UserEmail,
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt,
    Guid? AssignedByUserId);
