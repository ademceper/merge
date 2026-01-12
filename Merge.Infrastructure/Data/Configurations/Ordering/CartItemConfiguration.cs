using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Ordering;

namespace Merge.Infrastructure.Data.Configurations.Ordering;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasOne(e => e.Cart)
              .WithMany(e => e.CartItems)
              .HasForeignKey(e => e.CartId)
              .OnDelete(DeleteBehavior.Cascade);
              
        builder.HasOne(e => e.Product)
              .WithMany(e => e.CartItems)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasOne(e => e.ProductVariant)
              .WithMany()
              .HasForeignKey(e => e.ProductVariantId)
              .OnDelete(DeleteBehavior.SetNull);
              
        builder.Property(e => e.Price).HasPrecision(18, 2);
    }
}
