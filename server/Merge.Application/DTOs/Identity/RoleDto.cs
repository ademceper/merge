using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Identity;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    RoleType RoleType,
    bool IsSystemRole,
    DateTime CreatedAt,
    List<PermissionDto> Permissions);
