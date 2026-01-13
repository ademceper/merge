using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// LiveStreamProduct Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class LiveStreamProductConfiguration : IEntityTypeConfiguration<LiveStreamProduct>
{
    public void Configure(EntityTypeBuilder<LiveStreamProduct> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.LiveStreamId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.IsHighlighted);
        builder.HasIndex(e => new { e.LiveStreamId, e.ProductId }).IsUnique();
        builder.HasIndex(e => new { e.LiveStreamId, e.DisplayOrder });
        
        // Property configurations
        builder.Property(e => e.ShowcaseNotes)
            .HasMaxLength(1000);
        
        // ✅ BOLUM 1.3: Value Objects - Money (decimal precision)
        builder.Property(e => e.SpecialPrice)
            .HasPrecision(18, 2);
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // ✅ BOLUM 6.2: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LiveStreamProduct_DisplayOrder_NonNegative", "\"DisplayOrder\" >= 0");
            t.HasCheckConstraint("CK_LiveStreamProduct_ViewCount_NonNegative", "\"ViewCount\" >= 0");
            t.HasCheckConstraint("CK_LiveStreamProduct_ClickCount_NonNegative", "\"ClickCount\" >= 0");
            t.HasCheckConstraint("CK_LiveStreamProduct_OrderCount_NonNegative", "\"OrderCount\" >= 0");
            t.HasCheckConstraint("CK_LiveStreamProduct_SpecialPrice_NonNegative", "\"SpecialPrice\" >= 0 OR \"SpecialPrice\" IS NULL");
        });
        
        // Navigation properties
        builder.HasOne(e => e.LiveStream)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
