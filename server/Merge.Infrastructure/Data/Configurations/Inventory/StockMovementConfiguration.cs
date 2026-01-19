using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Inventory;

namespace Merge.Infrastructure.Data.Configurations.Inventory;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasOne(e => e.Inventory)
              .WithMany(e => e.StockMovements)
              .HasForeignKey(e => e.InventoryId)
              .OnDelete(DeleteBehavior.Cascade);
        
        // Warehouse relationship (WarehouseId -> Warehouse)
        builder.HasOne(e => e.Warehouse)
            .WithMany()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // FromWarehouse relationship (FromWarehouseId -> FromWarehouse)
        builder.HasOne(e => e.FromWarehouse)
            .WithMany()
            .HasForeignKey(e => e.FromWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // ToWarehouse relationship (ToWarehouseId -> ToWarehouse)
        builder.HasOne(e => e.ToWarehouse)
            .WithMany()
            .HasForeignKey(e => e.ToWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Product relationship
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // User relationship
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.PerformedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
