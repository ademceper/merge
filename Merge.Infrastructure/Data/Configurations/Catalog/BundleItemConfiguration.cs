using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class BundleItemConfiguration : IEntityTypeConfiguration<BundleItem>
{
    public void Configure(EntityTypeBuilder<BundleItem> builder)
    {
        builder.HasIndex(e => e.BundleId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => new { e.BundleId, e.ProductId }).IsUnique();
        
        builder.HasOne(e => e.Bundle)
              .WithMany()
              .HasForeignKey(e => e.BundleId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
