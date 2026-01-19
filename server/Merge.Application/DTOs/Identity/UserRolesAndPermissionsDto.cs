namespace Merge.Application.DTOs.Identity;

public record UserRolesAndPermissionsDto(
    List<string> PlatformRoles,
    List<StoreRoleInfo> StoreRoles,
    List<OrganizationRoleInfo> OrganizationRoles,
    List<StoreCustomerRoleInfo> StoreCustomerRoles,
    List<string> Permissions);

public record StoreRoleInfo(
    Guid StoreId,
    string StoreName,
    string RoleName,
    Guid RoleId);

public record OrganizationRoleInfo(
    Guid OrganizationId,
    string OrganizationName,
    string RoleName,
    Guid RoleId);

public record StoreCustomerRoleInfo(
    Guid StoreId,
    string StoreName,
    string RoleName,
    Guid RoleId);
