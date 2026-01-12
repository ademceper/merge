using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class VirtualTryOnConfiguration : IEntityTypeConfiguration<VirtualTryOn>
{
    public void Configure(EntityTypeBuilder<VirtualTryOn> builder)
    {
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.ProductId, e.UserId }).IsUnique();
        builder.HasIndex(e => e.IsEnabled);
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(e => e.Height).HasPrecision(18, 2);
        builder.Property(e => e.Chest).HasPrecision(18, 2);
        builder.Property(e => e.Waist).HasPrecision(18, 2);
        builder.Property(e => e.Hips).HasPrecision(18, 2);
    }
}
