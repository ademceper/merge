using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

/// <summary>
/// LiveStreamOrder Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class LiveStreamOrderConfiguration : IEntityTypeConfiguration<LiveStreamOrder>
{
    public void Configure(EntityTypeBuilder<LiveStreamOrder> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.LiveStreamId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => new { e.LiveStreamId, e.OrderId }).IsUnique();
        
        // ✅ BOLUM 1.3: Value Objects - Money (decimal precision)
        builder.Property(e => e.OrderAmount)
            .HasPrecision(18, 2);
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // ✅ BOLUM 6.2: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LiveStreamOrder_OrderAmount_Positive", "\"OrderAmount\" > 0");
        });
        
        // Navigation properties
        builder.HasOne(e => e.LiveStream)
            .WithMany(e => e.Orders)
            .HasForeignKey(e => e.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
