using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;
using Inventory = Merge.Domain.Modules.Inventory.Inventory;
using User = Merge.Domain.Modules.Identity.User;
using Role = Merge.Domain.Modules.Identity.Role;

namespace Merge.Infrastructure.Data.Contexts;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options), IDbContext
{

    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<PickPack> PickPacks { get; set; }
    public DbSet<PickPackItem> PickPackItems { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    // ✅ LSP FIX: Anlamlı hata mesajı - ISP gerektirir
    DbSet<User> IDbContext.Users =>
        throw new InvalidOperationException("InventoryDbContext does not support Users. Use ApplicationDbContext for identity operations.");
    DbSet<Role> IDbContext.Roles =>
        throw new InvalidOperationException("InventoryDbContext does not support Roles. Use ApplicationDbContext for identity operations.");
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles =>
        throw new InvalidOperationException("InventoryDbContext does not support UserRoles. Use ApplicationDbContext for identity operations.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Inventory");

        // ✅ HIGH-DB-003 FIX: Global Query Filter - Soft Delete (ZORUNLU)
        ConfigureGlobalQueryFilters(modelBuilder);
    }

    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
