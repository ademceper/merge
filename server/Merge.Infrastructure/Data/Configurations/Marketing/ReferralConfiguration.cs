using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Merge.Domain.Modules.Marketing;

namespace Merge.Infrastructure.Data.Configurations.Marketing;


public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.Property(e => e.ReferralCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(e => e.ReferrerId);
        builder.HasIndex(e => e.ReferredUserId);
        builder.HasIndex(e => e.ReferralCodeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.FirstOrderId);
        builder.HasIndex(e => new { e.ReferrerId, e.ReferredUserId }).IsUnique();
        builder.HasIndex(e => new { e.ReferrerId, e.Status });
        builder.HasIndex(e => new { e.ReferredUserId, e.Status });

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

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Referral_PointsAwarded_NonNegative", "\"PointsAwarded\" >= 0");
            t.HasCheckConstraint("CK_Referral_ReferrerId_NotEqual_ReferredUserId", "\"ReferrerId\" != \"ReferredUserId\"");
        });
    }
}
