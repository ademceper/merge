using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasOne(e => e.User)
              .WithMany(e => e.Reviews)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
              
        builder.HasOne(e => e.Product)
              .WithMany(e => e.Reviews)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.ProductId });
        builder.HasIndex(e => new { e.ProductId, e.IsApproved });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Review_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5");
        });
    }
}
