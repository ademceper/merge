using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

/// <summary>
/// UserCurrencyPreference Entity Configuration - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// </summary>
public class UserCurrencyPreferenceConfiguration : IEntityTypeConfiguration<UserCurrencyPreference>
{
    public void Configure(EntityTypeBuilder<UserCurrencyPreference> builder)
    {
        // ✅ BOLUM 6.1: Index Strategy - Unique constraint for UserId (one preference per user)
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.CurrencyId);
        builder.HasIndex(e => e.CurrencyCode);
        
        // ✅ BOLUM 1.7: Concurrency Control - RowVersion configuration
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
