using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;


public class UserLanguagePreferenceConfiguration : IEntityTypeConfiguration<UserLanguagePreference>
{
    public void Configure(EntityTypeBuilder<UserLanguagePreference> builder)
    {
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.LanguageId);
        builder.HasIndex(e => e.LanguageCode);
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.LanguageCode)
            .IsRequired()
            .HasMaxLength(10);
        
        // Navigation properties
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Language)
            .WithMany()
            .HasForeignKey(e => e.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
