using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// LoyaltyAccount EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.TierId);
        builder.HasIndex(e => new { e.UserId, e.TierId });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Tier)
            .WithMany()
            .HasForeignKey(e => e.TierId)
            .OnDelete(DeleteBehavior.SetNull);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LoyaltyAccount_PointsBalance_NonNegative", "\"PointsBalance\" >= 0");
            t.HasCheckConstraint("CK_LoyaltyAccount_LifetimePoints_NonNegative", "\"LifetimePoints\" >= 0");
        });
    }
}
