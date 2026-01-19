using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Enums;

namespace Merge.Infrastructure.Data.Seeders;

/// <summary>
/// Permission seeder - Varsayılan izinleri oluşturur
/// </summary>
public static class PermissionSeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken ct = default)
    {
        var permissions = GetDefaultPermissions();
        var existingPermissions = await context.Set<Permission>()
            .AsNoTracking()
            .Select(p => p.Name)
            .ToListAsync(ct);

        var newPermissions = permissions
            .Where(p => !existingPermissions.Contains(p.Name))
            .ToList();

        if (newPermissions.Count > 0)
        {
            await context.Set<Permission>().AddRangeAsync(newPermissions, ct);
            await context.SaveChangesAsync(ct);
        }
    }

    private static List<Permission> GetDefaultPermissions()
    {
        var permissions = new List<Permission>();

        // Products permissions
        permissions.Add(Permission.Create("products.view", "Products", "products", "view", "View products", true));
        permissions.Add(Permission.Create("products.create", "Products", "products", "create", "Create products", true));
        permissions.Add(Permission.Create("products.update", "Products", "products", "update", "Update products", true));
        permissions.Add(Permission.Create("products.delete", "Products", "products", "delete", "Delete products", true));
        permissions.Add(Permission.Create("products.publish", "Products", "products", "publish", "Publish products", true));
        permissions.Add(Permission.Create("products.unpublish", "Products", "products", "unpublish", "Unpublish products", true));

        // Orders permissions
        permissions.Add(Permission.Create("orders.view", "Orders", "orders", "view", "View orders", true));
        permissions.Add(Permission.Create("orders.create", "Orders", "orders", "create", "Create orders", true));
        permissions.Add(Permission.Create("orders.update", "Orders", "orders", "update", "Update orders", true));
        permissions.Add(Permission.Create("orders.cancel", "Orders", "orders", "cancel", "Cancel orders", true));
        permissions.Add(Permission.Create("orders.ship", "Orders", "orders", "ship", "Ship orders", true));
        permissions.Add(Permission.Create("orders.refund", "Orders", "orders", "refund", "Refund orders", true));

        // Users permissions
        permissions.Add(Permission.Create("users.view", "Users", "users", "view", "View users", true));
        permissions.Add(Permission.Create("users.create", "Users", "users", "create", "Create users", true));
        permissions.Add(Permission.Create("users.update", "Users", "users", "update", "Update users", true));
        permissions.Add(Permission.Create("users.delete", "Users", "users", "delete", "Delete users", true));
        permissions.Add(Permission.Create("users.activate", "Users", "users", "activate", "Activate users", true));
        permissions.Add(Permission.Create("users.deactivate", "Users", "users", "deactivate", "Deactivate users", true));

        // Roles permissions
        permissions.Add(Permission.Create("roles.view", "Roles", "roles", "view", "View roles", true));
        permissions.Add(Permission.Create("roles.create", "Roles", "roles", "create", "Create roles", true));
        permissions.Add(Permission.Create("roles.update", "Roles", "roles", "update", "Update roles", true));
        permissions.Add(Permission.Create("roles.delete", "Roles", "roles", "delete", "Delete roles", true));
        permissions.Add(Permission.Create("roles.assign", "Roles", "roles", "assign", "Assign roles to users", true));

        // Permissions permissions
        permissions.Add(Permission.Create("permissions.view", "Permissions", "permissions", "view", "View permissions", true));
        permissions.Add(Permission.Create("permissions.manage", "Permissions", "permissions", "manage", "Manage permissions", true));

        // Store permissions
        permissions.Add(Permission.Create("stores.view", "Stores", "stores", "view", "View stores", true));
        permissions.Add(Permission.Create("stores.create", "Stores", "stores", "create", "Create stores", true));
        permissions.Add(Permission.Create("stores.update", "Stores", "stores", "update", "Update stores", true));
        permissions.Add(Permission.Create("stores.delete", "Stores", "stores", "delete", "Delete stores", true));
        permissions.Add(Permission.Create("stores.manage", "Stores", "stores", "manage", "Manage store settings", true));

        // Store roles permissions
        permissions.Add(Permission.Create("store-roles.view", "Store Roles", "store-roles", "view", "View store roles", true));
        permissions.Add(Permission.Create("store-roles.assign", "Store Roles", "store-roles", "assign", "Assign store roles", true));
        permissions.Add(Permission.Create("store-roles.remove", "Store Roles", "store-roles", "remove", "Remove store roles", true));

        // Organization permissions
        permissions.Add(Permission.Create("organizations.view", "Organizations", "organizations", "view", "View organizations", true));
        permissions.Add(Permission.Create("organizations.create", "Organizations", "organizations", "create", "Create organizations", true));
        permissions.Add(Permission.Create("organizations.update", "Organizations", "organizations", "update", "Update organizations", true));
        permissions.Add(Permission.Create("organizations.delete", "Organizations", "organizations", "delete", "Delete organizations", true));
        permissions.Add(Permission.Create("organizations.manage", "Organizations", "organizations", "manage", "Manage organization settings", true));

        // Organization roles permissions
        permissions.Add(Permission.Create("organization-roles.view", "Organization Roles", "organization-roles", "view", "View organization roles", true));
        permissions.Add(Permission.Create("organization-roles.assign", "Organization Roles", "organization-roles", "assign", "Assign organization roles", true));
        permissions.Add(Permission.Create("organization-roles.remove", "Organization Roles", "organization-roles", "remove", "Remove organization roles", true));

        // Store customer roles permissions
        permissions.Add(Permission.Create("store-customer-roles.view", "Store Customer Roles", "store-customer-roles", "view", "View store customer roles", true));
        permissions.Add(Permission.Create("store-customer-roles.assign", "Store Customer Roles", "store-customer-roles", "assign", "Assign store customer roles", true));
        permissions.Add(Permission.Create("store-customer-roles.remove", "Store Customer Roles", "store-customer-roles", "remove", "Remove store customer roles", true));

        // Categories permissions
        permissions.Add(Permission.Create("categories.view", "Categories", "categories", "view", "View categories", true));
        permissions.Add(Permission.Create("categories.create", "Categories", "categories", "create", "Create categories", true));
        permissions.Add(Permission.Create("categories.update", "Categories", "categories", "update", "Update categories", true));
        permissions.Add(Permission.Create("categories.delete", "Categories", "categories", "delete", "Delete categories", true));

        // Payments permissions
        permissions.Add(Permission.Create("payments.view", "Payments", "payments", "view", "View payments", true));
        permissions.Add(Permission.Create("payments.process", "Payments", "payments", "process", "Process payments", true));
        permissions.Add(Permission.Create("payments.refund", "Payments", "payments", "refund", "Refund payments", true));

        // Analytics permissions
        permissions.Add(Permission.Create("analytics.view", "Analytics", "analytics", "view", "View analytics", true));
        permissions.Add(Permission.Create("analytics.export", "Analytics", "analytics", "export", "Export analytics", true));

        // Settings permissions
        permissions.Add(Permission.Create("settings.view", "Settings", "settings", "view", "View settings", true));
        permissions.Add(Permission.Create("settings.update", "Settings", "settings", "update", "Update settings", true));

        return permissions;
    }
}
