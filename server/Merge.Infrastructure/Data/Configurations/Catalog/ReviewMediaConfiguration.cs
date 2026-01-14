using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ReviewMediaConfiguration : IEntityTypeConfiguration<ReviewMedia>
{
    public void Configure(EntityTypeBuilder<ReviewMedia> builder)
    {
        builder.HasIndex(e => e.ReviewId);
        builder.HasIndex(e => new { e.ReviewId, e.DisplayOrder });
        
        builder.HasOne(e => e.Review)
              .WithMany()
              .HasForeignKey(e => e.ReviewId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
