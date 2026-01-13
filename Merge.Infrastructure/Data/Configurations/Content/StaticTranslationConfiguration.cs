using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Content;

namespace Merge.Infrastructure.Data.Configurations.Content;

/// <summary>
/// StaticTranslation Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class StaticTranslationConfiguration : IEntityTypeConfiguration<StaticTranslation>
{
    public void Configure(EntityTypeBuilder<StaticTranslation> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy - Unique constraint for Key + LanguageId
        builder.HasIndex(e => new { e.Key, e.LanguageId }).IsUnique();
        builder.HasIndex(e => e.LanguageId);
        builder.HasIndex(e => e.LanguageCode);
        builder.HasIndex(e => e.Category);
        
        // ✅ BOLUM 6.1: Index Strategy - Composite index for common queries
        builder.HasIndex(e => new { e.LanguageCode, e.Category });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(5000);
        
        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(50);
        
        // Navigation properties
        builder.HasOne(e => e.Language)
            .WithMany()
            .HasForeignKey(e => e.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_StaticTranslation_Key_Length", "LENGTH(\"Key\") > 0 AND LENGTH(\"Key\") <= 200");
            t.HasCheckConstraint("CK_StaticTranslation_Value_Length", "LENGTH(\"Value\") > 0 AND LENGTH(\"Value\") <= 5000");
        });
    }
}
