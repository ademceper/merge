using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// ReferralCode EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class ReferralCodeConfiguration : IEntityTypeConfiguration<ReferralCode>
{
    public void Configure(EntityTypeBuilder<ReferralCode> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.UserId, e.IsActive });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ReferralCode_UsageCount_NonNegative", "\"UsageCount\" >= 0");
            t.HasCheckConstraint("CK_ReferralCode_MaxUsage_NonNegative", "\"MaxUsage\" >= 0");
            t.HasCheckConstraint("CK_ReferralCode_PointsReward_NonNegative", "\"PointsReward\" >= 0");
            t.HasCheckConstraint("CK_ReferralCode_DiscountPercentage_Range", "\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100");
        });
    }
}
