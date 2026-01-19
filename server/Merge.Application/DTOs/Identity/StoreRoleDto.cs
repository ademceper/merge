namespace Merge.Application.DTOs.Identity;

public record StoreRoleDto(
    Guid Id,
    Guid StoreId,
    string StoreName,
    Guid UserId,
    string UserEmail,
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt,
    Guid? AssignedByUserId);
