using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.LanguageId);
        builder.HasIndex(e => new { e.ProductId, e.LanguageId }).IsUnique();
        
        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Language)
              .WithMany()
              .HasForeignKey(e => e.LanguageId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
