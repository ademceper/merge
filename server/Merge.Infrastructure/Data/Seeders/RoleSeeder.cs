using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Seeders;

/// <summary>
/// Role seeder - Varsayılan rolleri ve izinlerini oluşturur
/// </summary>
public static class RoleSeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken ct = default)
    {
        // Önce Permission'ları seed et
        await PermissionSeeder.SeedAsync(context, ct);

        var permissions = await context.Set<Permission>()
            .AsNoTracking()
            .ToListAsync(ct);

        var existingRoles = await context.Set<Role>()
            .AsNoTracking()
            .Select(r => r.Name)
            .ToListAsync(ct);

        var roles = GetDefaultRoles(permissions);
        var newRoles = roles
            .Where(r => !existingRoles.Contains(r.Name))
            .ToList();

        if (newRoles.Count > 0)
        {
            await context.Set<Role>().AddRangeAsync(newRoles, ct);
            await context.SaveChangesAsync(ct);

            // RolePermission'ları ekle
            var savedRoles = await context.Set<Role>()
                .Where(r => newRoles.Select(nr => nr.Name).Contains(r.Name))
                .ToListAsync(ct);

            var rolePermissions = new List<RolePermission>();

            foreach (var role in savedRoles)
            {
                var roleDefinition = GetDefaultRoles(permissions).First(r => r.Name == role.Name);
                var permissionNames = GetRolePermissions(role.Name);

                var rolePerms = permissions
                    .Where(p => permissionNames.Contains(p.Name))
                    .Select(p => RolePermission.Create(role.Id, p.Id))
                    .ToList();

                rolePermissions.AddRange(rolePerms);
            }

            if (rolePermissions.Count > 0)
            {
                await context.Set<RolePermission>().AddRangeAsync(rolePermissions, ct);
                await context.SaveChangesAsync(ct);
            }
        }
    }

    private static List<Role> GetDefaultRoles(List<Permission> permissions)
    {
        var roles = new List<Role>();

        // Platform roles
        roles.Add(Role.Create("SuperAdmin", RoleType.Platform, "Super administrator with all permissions", true));
        roles.Add(Role.Create("Admin", RoleType.Platform, "Platform administrator", true));
        roles.Add(Role.Create("Moderator", RoleType.Platform, "Platform moderator", true));
        roles.Add(Role.Create("Customer", RoleType.Platform, "Regular customer", true));
        roles.Add(Role.Create("Seller", RoleType.Platform, "Marketplace seller", true));

        // Store roles
        roles.Add(Role.Create("Store Owner", RoleType.Store, "Store owner with full control", true));
        roles.Add(Role.Create("Store Manager", RoleType.Store, "Store manager", true));
        roles.Add(Role.Create("Store Staff", RoleType.Store, "Store staff member", true));

        // Organization roles
        roles.Add(Role.Create("Organization Admin", RoleType.Organization, "Organization administrator", true));
        roles.Add(Role.Create("Organization Member", RoleType.Organization, "Organization member", true));

        // Store customer roles
        roles.Add(Role.Create("VIP Customer", RoleType.StoreCustomer, "VIP customer in store", true));
        roles.Add(Role.Create("Regular Customer", RoleType.StoreCustomer, "Regular customer in store", true));

        return roles;
    }

    private static List<string> GetRolePermissions(string roleName)
    {
        return roleName switch
        {
            "SuperAdmin" => [
                // Tüm izinler
            ],
            "Admin" => [
                "users.view", "users.create", "users.update", "users.activate", "users.deactivate",
                "roles.view", "roles.create", "roles.update", "roles.assign",
                "permissions.view", "permissions.manage",
                "stores.view", "stores.create", "stores.update", "stores.delete",
                "organizations.view", "organizations.create", "organizations.update", "organizations.delete",
                "analytics.view", "analytics.export",
                "settings.view", "settings.update"
            ],
            "Moderator" => [
                "users.view", "users.activate", "users.deactivate",
                "products.view", "products.update", "products.publish", "products.unpublish",
                "orders.view", "orders.update",
                "categories.view", "categories.create", "categories.update"
            ],
            "Customer" => [
                "products.view",
                "orders.view", "orders.create",
                "categories.view"
            ],
            "Seller" => [
                "products.view", "products.create", "products.update", "products.delete", "products.publish", "products.unpublish",
                "orders.view",
                "stores.view", "stores.create", "stores.update", "stores.manage",
                "store-roles.view", "store-roles.assign", "store-roles.remove",
                "store-customer-roles.view", "store-customer-roles.assign", "store-customer-roles.remove",
                "analytics.view"
            ],
            "Store Owner" => [
                "products.view", "products.create", "products.update", "products.delete", "products.publish", "products.unpublish",
                "orders.view", "orders.update", "orders.ship",
                "stores.view", "stores.update", "stores.manage",
                "store-roles.view", "store-roles.assign", "store-roles.remove",
                "store-customer-roles.view", "store-customer-roles.assign", "store-customer-roles.remove",
                "analytics.view", "analytics.export"
            ],
            "Store Manager" => [
                "products.view", "products.create", "products.update", "products.publish", "products.unpublish",
                "orders.view", "orders.update", "orders.ship",
                "store-roles.view",
                "analytics.view"
            ],
            "Store Staff" => [
                "products.view", "products.update",
                "orders.view", "orders.update"
            ],
            "Organization Admin" => [
                "organizations.view", "organizations.update", "organizations.manage",
                "organization-roles.view", "organization-roles.assign", "organization-roles.remove",
                "users.view", "users.create", "users.update"
            ],
            "Organization Member" => [
                "organizations.view",
                "users.view"
            ],
            "VIP Customer" => [
                "products.view",
                "orders.view", "orders.create"
            ],
            "Regular Customer" => [
                "products.view",
                "orders.view", "orders.create"
            ],
            _ => []
        };
    }
}
