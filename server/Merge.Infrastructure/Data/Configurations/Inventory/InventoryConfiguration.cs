using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Merge.Infrastructure.Data.Configurations.Inventory;

public class InventoryConfiguration : IEntityTypeConfiguration<Merge.Domain.Modules.Inventory.Inventory>
{
    public void Configure(EntityTypeBuilder<Merge.Domain.Modules.Inventory.Inventory> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UnitCost).HasPrecision(18, 2);
        
        builder.HasOne(e => e.Product)
              .WithMany() // Assuming Product doesn't have a collection of Inventory (per-warehouse)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Warehouse)
              .WithMany(e => e.Inventories)
              .HasForeignKey(e => e.WarehouseId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
