namespace Merge.Application.DTOs.Identity;

public record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    string Category,
    string Resource,
    string Action,
    bool IsSystemPermission,
    DateTime CreatedAt);
