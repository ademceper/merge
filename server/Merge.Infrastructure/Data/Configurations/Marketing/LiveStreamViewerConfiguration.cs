using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class LiveStreamViewerConfiguration : IEntityTypeConfiguration<LiveStreamViewer>
{
    public void Configure(EntityTypeBuilder<LiveStreamViewer> builder)
    {
        builder.HasIndex(e => e.LiveStreamId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.GuestId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.JoinedAt);
        builder.HasIndex(e => new { e.LiveStreamId, e.UserId });
        builder.HasIndex(e => new { e.LiveStreamId, e.GuestId });
        
        // Property configurations
        builder.Property(e => e.GuestId)
            .HasMaxLength(100);
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LiveStreamViewer_WatchDuration_NonNegative", "\"WatchDuration\" >= 0");
            t.HasCheckConstraint("CK_LiveStreamViewer_LeftAt_After_JoinedAt", 
                "\"LeftAt\" IS NULL OR \"LeftAt\" >= \"JoinedAt\"");
        });
        
        // Navigation properties
        builder.HasOne(e => e.LiveStream)
            .WithMany(e => e.Viewers)
            .HasForeignKey(e => e.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
