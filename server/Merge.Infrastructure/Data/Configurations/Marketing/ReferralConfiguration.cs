using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// Referral EF Core Configuration - BOLUM 8.0: EF Core Configuration (ZORUNLU)
/// </summary>
public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        // ✅ BOLUM 8.1: Property Configuration
        builder.Property(e => e.ReferralCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // ✅ BOLUM 8.2: Index Configuration
        builder.HasIndex(e => e.ReferrerId);
        builder.HasIndex(e => e.ReferredUserId);
        builder.HasIndex(e => e.ReferralCodeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.FirstOrderId);
        builder.HasIndex(e => new { e.ReferrerId, e.ReferredUserId }).IsUnique();
        builder.HasIndex(e => new { e.ReferrerId, e.Status });
        builder.HasIndex(e => new { e.ReferredUserId, e.Status });

        // ✅ BOLUM 8.3: Relationship Configuration
        builder.HasOne(e => e.Referrer)
            .WithMany()
            .HasForeignKey(e => e.ReferrerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReferredUser)
            .WithMany()
            .HasForeignKey(e => e.ReferredUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReferralCodeEntity)
            .WithMany()
            .HasForeignKey(e => e.ReferralCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FirstOrder)
            .WithMany()
            .HasForeignKey(e => e.FirstOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // ✅ BOLUM 8.4: Check Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Referral_PointsAwarded_NonNegative", "\"PointsAwarded\" >= 0");
            t.HasCheckConstraint("CK_Referral_ReferrerId_NotEqual_ReferredUserId", "\"ReferrerId\" != \"ReferredUserId\"");
        });
    }
}
