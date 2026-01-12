using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasOne(e => e.Product)
              .WithMany(e => e.Variants)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
              
        builder.Property(e => e.Price).HasPrecision(18, 2);
    }
}
