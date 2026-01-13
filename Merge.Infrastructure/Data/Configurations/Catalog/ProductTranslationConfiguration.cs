using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Catalog;

namespace Merge.Infrastructure.Data.Configurations.Catalog;

/// <summary>
/// ProductTranslation Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.LanguageId);
        builder.HasIndex(e => e.LanguageCode);
        builder.HasIndex(e => new { e.ProductId, e.LanguageId }).IsUnique();
        builder.HasIndex(e => new { e.ProductId, e.LanguageCode }).IsUnique();
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Description)
            .HasMaxLength(5000);
        
        builder.Property(e => e.ShortDescription)
            .HasMaxLength(500);
        
        builder.Property(e => e.MetaTitle)
            .HasMaxLength(200);
        
        builder.Property(e => e.MetaDescription)
            .HasMaxLength(500);
        
        builder.Property(e => e.MetaKeywords)
            .HasMaxLength(200);
        
        // Navigation properties
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
