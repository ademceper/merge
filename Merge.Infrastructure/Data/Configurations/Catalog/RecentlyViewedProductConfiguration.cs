using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class RecentlyViewedProductConfiguration : IEntityTypeConfiguration<RecentlyViewedProduct>
{
    public void Configure(EntityTypeBuilder<RecentlyViewedProduct> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => new { e.UserId, e.ProductId });
        builder.HasIndex(e => e.ViewedAt);
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
