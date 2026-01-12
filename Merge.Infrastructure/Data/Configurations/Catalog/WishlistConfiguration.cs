using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

        builder.HasOne(e => e.User)
              .WithMany(e => e.Wishlists)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
              
        builder.HasOne(e => e.Product)
              .WithMany(e => e.Wishlists)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
