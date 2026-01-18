using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductBundleConfiguration : IEntityTypeConfiguration<ProductBundle>
{
    public void Configure(EntityTypeBuilder<ProductBundle> builder)
    {
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsActive);
        
        builder.Property(e => e.BundlePrice).HasPrecision(18, 2);
        builder.Property(e => e.OriginalTotalPrice).HasPrecision(18, 2);
        builder.Property(e => e.DiscountPercentage).HasPrecision(5, 2); // 0-100 arası yüzde
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.BundleItems)
              .WithOne(e => e.Bundle)
              .HasForeignKey(e => e.BundleId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
