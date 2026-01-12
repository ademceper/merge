using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(e => e.SKU).IsUnique();
        builder.HasIndex(e => e.SellerId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.CategoryId, e.IsActive });
        
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.DiscountPrice).HasPrecision(18, 2);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Product_Price_Positive", "\"Price\" >= 0");
            t.HasCheckConstraint("CK_Product_Stock_NonNegative", "\"StockQuantity\" >= 0");
        });
        
        builder.HasOne(e => e.Category)
              .WithMany(e => e.Products)
              .HasForeignKey(e => e.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasOne(e => e.Seller)
              .WithMany()
              .HasForeignKey(e => e.SellerId)
              .OnDelete(DeleteBehavior.SetNull);
              
        builder.HasOne(e => e.Store)
              .WithMany(e => e.Products)
              .HasForeignKey(e => e.StoreId)
              .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(e => e.ImageUrls)
              .HasConversion(
                  v => string.Join(',', v),
                  v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}
