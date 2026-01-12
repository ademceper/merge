using Microsoft.EntityFrameworkCore;
using Merge.Domain.Modules.Inventory;
using Merge.Application.Interfaces;
using Merge.Domain.SharedKernel;

namespace Merge.Infrastructure.Data.Contexts;

public class InventoryDbContext : DbContext, IDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Merge.Domain.Modules.Inventory.Inventory> Inventories { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<PickPack> PickPacks { get; set; }
    public DbSet<PickPackItem> PickPackItems { get; set; }

    DbSet<TEntity> IDbContext.Set<TEntity>() => base.Set<TEntity>();
    
    DbSet<Merge.Domain.Modules.Identity.User> IDbContext.Users => throw new NotImplementedException();
    DbSet<Merge.Domain.Modules.Identity.Role> IDbContext.Roles => throw new NotImplementedException();
    DbSet<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>> IDbContext.UserRoles => throw new NotImplementedException();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly, 
            type => type.Namespace == "Merge.Infrastructure.Data.Configurations.Inventory");
    }
}
