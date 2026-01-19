using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class OrderSplitConfiguration : IEntityTypeConfiguration<OrderSplit>
{
    public void Configure(EntityTypeBuilder<OrderSplit> builder)
    {
        // OriginalOrder relationship (OriginalOrderId -> OriginalOrder.OriginalSplits)
        builder.HasOne(e => e.OriginalOrder)
            .WithMany(o => o.OriginalSplits)
            .HasForeignKey(e => e.OriginalOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // SplitOrder relationship (SplitOrderId -> SplitOrder.SplitFrom)
        builder.HasOne(e => e.SplitOrder)
            .WithMany(o => o.SplitFrom)
            .HasForeignKey(e => e.SplitOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
