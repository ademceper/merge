using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductTrustBadgeConfiguration : IEntityTypeConfiguration<ProductTrustBadge>
{
    public void Configure(EntityTypeBuilder<ProductTrustBadge> builder)
    {
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.TrustBadgeId);
        builder.HasIndex(e => new { e.ProductId, e.TrustBadgeId }).IsUnique();
        builder.HasIndex(e => e.IsActive);
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.TrustBadge)
              .WithMany()
              .HasForeignKey(e => e.TrustBadgeId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
