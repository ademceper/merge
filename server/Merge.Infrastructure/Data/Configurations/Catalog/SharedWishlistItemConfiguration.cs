using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class SharedWishlistItemConfiguration : IEntityTypeConfiguration<SharedWishlistItem>
{
    public void Configure(EntityTypeBuilder<SharedWishlistItem> builder)
    {
        builder.HasIndex(e => e.SharedWishlistId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => new { e.SharedWishlistId, e.ProductId }).IsUnique();
        
        builder.HasOne(e => e.SharedWishlist)
              .WithMany()
              .HasForeignKey(e => e.SharedWishlistId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.PurchasedByUser)
              .WithMany()
              .HasForeignKey(e => e.PurchasedBy)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
