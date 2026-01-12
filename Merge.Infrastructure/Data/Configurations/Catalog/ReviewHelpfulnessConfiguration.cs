using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ReviewHelpfulnessConfiguration : IEntityTypeConfiguration<ReviewHelpfulness>
{
    public void Configure(EntityTypeBuilder<ReviewHelpfulness> builder)
    {
        builder.HasIndex(e => e.ReviewId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
        
        builder.HasOne(e => e.Review)
              .WithMany()
              .HasForeignKey(e => e.ReviewId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
