using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductQuestionConfiguration : IEntityTypeConfiguration<ProductQuestion>
{
    public void Configure(EntityTypeBuilder<ProductQuestion> builder)
    {
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.ProductId, e.IsApproved });
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
        
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.Answers)
              .WithOne(e => e.Question)
              .HasForeignKey(e => e.QuestionId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.HelpfulnessVotes)
              .WithOne(e => e.Question)
              .HasForeignKey(e => e.QuestionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
