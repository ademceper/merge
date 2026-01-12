using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.LanguageId);
        builder.HasIndex(e => new { e.CategoryId, e.LanguageId }).IsUnique();
        
        builder.HasOne(e => e.Category)
              .WithMany()
              .HasForeignKey(e => e.CategoryId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Language)
              .WithMany()
              .HasForeignKey(e => e.LanguageId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
