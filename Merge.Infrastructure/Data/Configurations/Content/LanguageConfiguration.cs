using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Content;

namespace Merge.Infrastructure.Data.Configurations.Content;

/// <summary>
/// Language Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy - Unique constraint for Code
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.IsDefault);
        builder.HasIndex(e => e.IsActive);
        
        // ✅ BOLUM 6.1: Index Strategy - Composite index for common queries
        builder.HasIndex(e => new { e.IsActive, e.IsDefault });
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.NativeName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.FlagIcon)
            .HasMaxLength(500);
        
        // ✅ BOLUM 6.1: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Language_Code_Length", "LENGTH(\"Code\") >= 2 AND LENGTH(\"Code\") <= 10");
        });
    }
}
