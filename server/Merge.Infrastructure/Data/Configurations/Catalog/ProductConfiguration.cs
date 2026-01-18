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
        
        builder.HasIndex(e => e.StoreId);
        
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.DiscountPrice).HasPrecision(18, 2);
        builder.Property(e => e.Rating).HasPrecision(3, 2); // 0.00-5.00 arası rating
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Product_Price_Positive", "\"Price\" >= 0");
            t.HasCheckConstraint("CK_Product_Stock_NonNegative", "\"StockQuantity\" >= 0");
        });
        
        builder.HasOne(e => e.Category)
              .WithMany()
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
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.Reviews)
              .WithOne(e => e.Product)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.Variants)
              .WithOne(e => e.Product)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.Wishlists)
              .WithOne(e => e.Product)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.BundleItems)
              .WithOne(e => e.Product)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.RecentlyViewedProducts)
              .WithOne(e => e.Product)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Collection backing fields don't need conversion - EF Core handles them automatically
        // If conversion is needed, use ValueConverter:
        // var converter = new ValueConverter<List<string>, string>(
        //     v => string.Join(',', v),
        //     v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        // builder.Property("_imageUrls").HasConversion(converter);
    }
}
