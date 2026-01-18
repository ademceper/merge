using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.TierId);
        builder.HasIndex(e => new { e.UserId, e.TierId });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Tier)
            .WithMany()
            .HasForeignKey(e => e.TierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LoyaltyAccount_PointsBalance_NonNegative", "\"PointsBalance\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyAccount_LifetimePoints_NonNegative", "\"LifetimePoints\" >= 0");
        });
    }
}
