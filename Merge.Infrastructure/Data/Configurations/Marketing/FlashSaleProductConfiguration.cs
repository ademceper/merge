using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

public class FlashSaleProductConfiguration : IEntityTypeConfiguration<FlashSaleProduct>
{
    public void Configure(EntityTypeBuilder<FlashSaleProduct> builder)
    {
        builder.HasOne(e => e.FlashSale)
              .WithMany(e => e.FlashSaleProducts)
              .HasForeignKey(e => e.FlashSaleId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
              .WithMany(e => e.FlashSaleProducts)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.SalePrice).HasPrecision(18, 2);
        builder.HasIndex(e => new { e.FlashSaleId, e.ProductId });
    }
}
