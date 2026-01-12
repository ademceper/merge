using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductSizeGuideConfiguration : IEntityTypeConfiguration<ProductSizeGuide>
{
    public void Configure(EntityTypeBuilder<ProductSizeGuide> builder)
    {
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.SizeGuideId);
        builder.HasIndex(e => new { e.ProductId, e.SizeGuideId }).IsUnique();
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.SizeGuide)
              .WithMany()
              .HasForeignKey(e => e.SizeGuideId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
