using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;


public class UserCurrencyPreferenceConfiguration : IEntityTypeConfiguration<UserCurrencyPreference>
{
    public void Configure(EntityTypeBuilder<UserCurrencyPreference> builder)
    {
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.CurrencyId);
        builder.HasIndex(e => e.CurrencyCode);
        
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsRequired(false);
        
        // Property configurations
        builder.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);
        
        // Navigation properties
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
