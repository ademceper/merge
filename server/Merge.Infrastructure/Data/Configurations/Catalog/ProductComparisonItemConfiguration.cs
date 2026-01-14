using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductComparisonItemConfiguration : IEntityTypeConfiguration<ProductComparisonItem>
{
    public void Configure(EntityTypeBuilder<ProductComparisonItem> builder)
    {
        builder.HasIndex(e => e.ComparisonId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => new { e.ComparisonId, e.ProductId }).IsUnique();
        
        builder.HasOne(e => e.Comparison)
              .WithMany()
              .HasForeignKey(e => e.ComparisonId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
