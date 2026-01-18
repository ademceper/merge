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
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.HelpfulnessVotes)
              .WithOne(e => e.Review)
              .HasForeignKey(e => e.ReviewId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.Media)
              .WithOne(e => e.Review)
              .HasForeignKey(e => e.ReviewId)
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
