using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductAnswerConfiguration : IEntityTypeConfiguration<ProductAnswer>
{
    public void Configure(EntityTypeBuilder<ProductAnswer> builder)
    {
        builder.HasIndex(e => e.QuestionId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.QuestionId, e.IsApproved });
        
        builder.HasOne(e => e.Question)
              .WithMany()
              .HasForeignKey(e => e.QuestionId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
        
        // ✅ BOLUM 1.1: Backing field mapping for encapsulated collections
        // EF Core automatically discovers backing fields by convention (_fieldName)
        // Navigation property'ler IReadOnlyCollection olduğu için EF Core backing field'ları otomatik bulur
        builder.HasMany(e => e.HelpfulnessVotes)
              .WithOne(e => e.Answer)
              .HasForeignKey(e => e.AnswerId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
