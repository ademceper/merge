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
    }
}
