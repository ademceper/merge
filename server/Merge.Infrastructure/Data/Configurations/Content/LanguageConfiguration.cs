using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Content;

namespace Merge.Infrastructure.Data.Configurations.Content;


public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.IsDefault);
        builder.HasIndex(e => e.IsActive);
        
        builder.HasIndex(e => new { e.IsActive, e.IsDefault });
        
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
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Language_Code_Length", "LENGTH(\"Code\") >= 2 AND LENGTH(\"Code\") <= 10");
        });
    }
}
