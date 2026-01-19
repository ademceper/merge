namespace Merge.Application.DTOs.Identity;

public record OrganizationRoleDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid UserId,
    string UserEmail,
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt,
    Guid? AssignedByUserId);
