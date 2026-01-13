using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// LiveStream Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class LiveStreamConfiguration : IEntityTypeConfiguration<LiveStream>
{
    public void Configure(EntityTypeBuilder<LiveStream> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.SellerId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.ScheduledStartTime);
        builder.HasIndex(e => new { e.SellerId, e.Status });
        builder.HasIndex(e => new { e.Status, e.IsActive });
        
        // Property configurations
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Description)
            .HasMaxLength(2000);
        
        builder.Property(e => e.StreamUrl)
            .HasMaxLength(500);
        
        builder.Property(e => e.StreamKey)
            .HasMaxLength(200);
        
        builder.Property(e => e.ThumbnailUrl)
            .HasMaxLength(500);
        
        builder.Property(e => e.Category)
            .HasMaxLength(100);
        
        builder.Property(e => e.Tags)
            .HasMaxLength(500);
        
        // ✅ BOLUM 1.3: Value Objects - Money (decimal precision)
        builder.Property(e => e.Revenue)
            .HasPrecision(18, 2);
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // ✅ BOLUM 6.2: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LiveStream_Revenue_NonNegative", "\"Revenue\" >= 0");
            t.HasCheckConstraint("CK_LiveStream_ViewerCount_NonNegative", "\"ViewerCount\" >= 0");
            t.HasCheckConstraint("CK_LiveStream_PeakViewerCount_NonNegative", "\"PeakViewerCount\" >= 0");
            t.HasCheckConstraint("CK_LiveStream_TotalViewerCount_NonNegative", "\"TotalViewerCount\" >= 0");
            t.HasCheckConstraint("CK_LiveStream_OrderCount_NonNegative", "\"OrderCount\" >= 0");
        });
        
        // Navigation properties
        builder.HasOne(e => e.Seller)
            .WithMany()
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ✅ BOLUM 1.1: Backing field mapping for encapsulated collections
        builder.HasMany(e => e.Products)
            .WithOne(e => e.LiveStream)
            .HasForeignKey(e => e.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(e => e.Viewers)
            .WithOne(e => e.LiveStream)
            .HasForeignKey(e => e.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(e => e.Orders)
            .WithOne(e => e.LiveStream)
            .HasForeignKey(e => e.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
