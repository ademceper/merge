using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class SizeGuideEntryConfiguration : IEntityTypeConfiguration<SizeGuideEntry>
{
    public void Configure(EntityTypeBuilder<SizeGuideEntry> builder)
    {
        builder.HasIndex(e => e.SizeGuideId);
        builder.HasIndex(e => new { e.SizeGuideId, e.SizeLabel });
        
        builder.HasOne(e => e.SizeGuide)
              .WithMany()
              .HasForeignKey(e => e.SizeGuideId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(e => e.Chest).HasPrecision(18, 2);
        builder.Property(e => e.Waist).HasPrecision(18, 2);
        builder.Property(e => e.Hips).HasPrecision(18, 2);
        builder.Property(e => e.Inseam).HasPrecision(18, 2);
        builder.Property(e => e.Shoulder).HasPrecision(18, 2);
        builder.Property(e => e.Length).HasPrecision(18, 2);
        builder.Property(e => e.Width).HasPrecision(18, 2);
        builder.Property(e => e.Height).HasPrecision(18, 2);
        builder.Property(e => e.Weight).HasPrecision(18, 2);
    }
}
