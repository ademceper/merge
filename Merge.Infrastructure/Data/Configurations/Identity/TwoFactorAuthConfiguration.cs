using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Identity;

namespace Merge.Infrastructure.Data.Configurations.Identity;

public class TwoFactorAuthConfiguration : IEntityTypeConfiguration<TwoFactorAuth>
{
    public void Configure(EntityTypeBuilder<TwoFactorAuth> builder)
    {
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasOne(e => e.User)
              .WithOne()
              .HasForeignKey<TwoFactorAuth>(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
              
        builder.Property(e => e.BackupCodes)
              .HasConversion(
                  v => v != null ? string.Join(',', v) : null,
                  v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries) : null);
    }
}
